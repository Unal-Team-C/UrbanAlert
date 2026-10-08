using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using Amazon;
using Amazon.S3;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using UrbanAlert.Auditoria;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string? issuer = builder.Configuration["Cognito:Issuer"];
string? appClientId = builder.Configuration["Cognito:AppClientId"];

if (!builder.Environment.IsDevelopment() && (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(appClientId)))
    throw new InvalidOperationException("Cognito:Issuer y Cognito:AppClientId son obligatorios fuera de Development.");

// Modo de desarrollo sin Cognito (AutenticacionDesarrolloHandler). Nunca fuera de Development.
bool modoDesarrollo = builder.Configuration.GetValue<bool>("Autenticacion:ModoDesarrollo");
if (modoDesarrollo && !builder.Environment.IsDevelopment())
    throw new InvalidOperationException("Autenticacion:ModoDesarrollo solo se permite en Development.");
const string EsquemaJwtODesarrollo = "JwtODesarrollo";

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    // En modo de desarrollo, Scalar muestra los headers que reemplazan al token de Cognito.
    if (modoDesarrollo)
        options.AddOperationTransformer(AutenticacionDesarrolloHandler.DocumentarHeadersAsync);
});
builder.Services.AddSingleton<IAuditStore, AuditStore>();
builder.Services.AddSingleton<IAuditAuthorizationService, AuditAuthorizationService>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<IAuditArchive>(services =>
{
    IConfiguration configuration = services.GetRequiredService<IConfiguration>();
    string? bucket = configuration["AuditArchive:Bucket"];
    if (string.IsNullOrWhiteSpace(bucket))
        return new NoOpAuditArchive();

    int retentionDays = configuration.GetValue<int?>("AuditArchive:ObjectLockRetentionDays") ?? 2555;
    if (retentionDays < 1)
        throw new InvalidOperationException("AuditArchive:ObjectLockRetentionDays debe ser mayor que cero.");

    AmazonS3Config s3Configuration = new()
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(configuration["AWS_REGION"] ?? "us-east-1")
    };
    string? serviceUrl = configuration["AuditArchive:ServiceUrl"];
    if (!string.IsNullOrWhiteSpace(serviceUrl))
    {
        s3Configuration.ServiceURL = serviceUrl;
        s3Configuration.ForcePathStyle = true;
    }

    return new S3AuditArchive(new AmazonS3Client(s3Configuration), bucket, retentionDays);
});
AuthenticationBuilder autenticacion = builder.Services
    .AddAuthentication(modoDesarrollo ? EsquemaJwtODesarrollo : JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = issuer;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
            ValidIssuer = issuer,
            ValidateAudience = false,
            NameClaimType = "sub"
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                string? tokenUse = context.Principal?.FindFirst("token_use")?.Value;
                bool validClient = tokenUse switch
                {
                    "access" => context.Principal?.FindFirst("client_id")?.Value == appClientId,
                    "id" => context.Principal?.FindAll("aud").Any(claim => claim.Value == appClientId) == true,
                    _ => false
                };
                if (!validClient)
                    context.Fail("Se requiere un token Cognito de Urban Alert.");
                return Task.CompletedTask;
            }
        };
    });
if (modoDesarrollo)
{
    // Con Authorization: Bearer se valida el token de Cognito como siempre; sin él, se usan los
    // headers de desarrollo.
    autenticacion
        .AddScheme<AuthenticationSchemeOptions, AutenticacionDesarrolloHandler>(AutenticacionDesarrolloHandler.Esquema, null)
        .AddPolicyScheme(EsquemaJwtODesarrollo, null, options => options.ForwardDefaultSelector = context =>
            context.Request.Headers.Authorization.Count > 0
                ? JwtBearerDefaults.AuthenticationScheme
                : AutenticacionDesarrolloHandler.Esquema);
}
builder.Services.AddAuthorization();

if (builder.Configuration.GetValue<bool>("RabbitMq:Enabled"))
{
    string rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
    ushort rabbitPort = builder.Configuration.GetValue<ushort?>("RabbitMq:Port") ?? 5672;
    string virtualHost = builder.Configuration["RabbitMq:VirtualHost"] ?? "/";
    string username = builder.Configuration["RabbitMq:Username"] ?? "guest";
    string password = builder.Configuration["RabbitMq:Password"] ?? "guest";
    string queue = builder.Configuration["RabbitMq:Queue"] ?? "q_auditoria_dotnet";
    // Amazon MQ (RabbitMQ administrado) exige TLS en el 5671; el RabbitMQ local no lo soporta.
    bool rabbitUsarTls = builder.Configuration.GetValue<bool>("RabbitMq:UseSsl");

    builder.Services.AddMassTransit(registration =>
    {
        registration.AddConsumer<ReporteCreadoConsumer>();
        registration.UsingRabbitMq((context, bus) =>
        {
            bus.Host(rabbitHost, rabbitPort, virtualHost, host =>
            {
                host.Username(username);
                host.Password(password);

                if (rabbitUsarTls)
                    host.UseSsl(s => s.Protocol = SslProtocols.Tls12);
            });
            bus.ConfigureJsonSerializerOptions(CodigosEnum.ConfigurarMensajeria);
            bus.ReceiveEndpoint(queue, endpoint =>
            {
                endpoint.UseMessageRetry(retry => retry.Intervals(
                    TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15)));
                endpoint.ConfigureConsumer<ReporteCreadoConsumer>(context);
            });
        });
    });
}

WebApplication app = builder.Build();

if (modoDesarrollo)
    app.Logger.LogWarning(
        "Modo de desarrollo sin Cognito: sin token, la identidad se toma de {HeaderUsuario} y {HeaderRol} (por defecto, admin).",
        AutenticacionDesarrolloHandler.HeaderUsuario, AutenticacionDesarrolloHandler.HeaderRol);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/health")
    {
        await next();
        return;
    }

    string? sharedSecret = app.Configuration["Gateway:SharedSecret"];
    if (string.IsNullOrWhiteSpace(sharedSecret))
    {
        if (!app.Environment.IsDevelopment())
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new { message = "La clave del gateway no está configurada." });
            return;
        }
    }
    else
    {
        byte[] expected = Encoding.UTF8.GetBytes(sharedSecret);
        byte[] supplied = Encoding.UTF8.GetBytes(context.Request.Headers["X-Urban-Gateway-Key"].ToString());
        if (expected.Length != supplied.Length || !CryptographicOperations.FixedTimeEquals(expected, supplied))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = "La solicitud debe ingresar por API Gateway." });
            return;
        }
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Run();

public partial class Program { }

using System.Text.Json.Serialization;
using API.Authentication;
using API.Middleware;
using Application;
using Infrastructure;
using Infrastructure.Persistencia;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    // Autenticación simplificada mientras se define el proveedor de identidad real;
    // autentica toda petición como un usuario Administrador fijo.
    builder.Services.AddAuthentication(DefaultUserAuthenticationHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DefaultUserAuthenticationHandler>(
            DefaultUserAuthenticationHandler.SchemeName, options => { });
}
else
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
}

builder.Services.AddAuthorization();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using IServiceScope scope = app.Services.CreateScope();
    ReportesDbContext dbContext = scope.ServiceProvider.GetRequiredService<ReportesDbContext>();
    dbContext.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

public partial class Program { }
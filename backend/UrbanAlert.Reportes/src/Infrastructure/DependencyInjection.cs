using Application.Interfaces.Eventos;
using Application.Interfaces.Reportes;
using Infrastructure.Mensajeria;
using Infrastructure.Persistencia;
using Infrastructure.Persistencia.Repositorios;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Postgres")
                                  ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'Postgres'.");

        services.AddDbContext<ReportesDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IReporteRepository, ReporteRepository>();

        AgregarMassTransit(services, configuration);

        return services;
    }

    private static void AgregarMassTransit(IServiceCollection services, IConfiguration configuration)
    {
        string host = configuration["RabbitMq:Host"]
                      ?? throw new InvalidOperationException("No se encontró la configuración 'RabbitMq:Host'.");
        string virtualHost = configuration["RabbitMq:VirtualHost"] ?? "/";
        string usuario = configuration["RabbitMq:Username"] ?? "guest";
        string contrasena = configuration["RabbitMq:Password"] ?? "guest";

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((_, cfg) =>
            {
                cfg.Host(host, virtualHost, h =>
                {
                    h.Username(usuario);
                    h.Password(contrasena);
                });
            });
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
    }
}

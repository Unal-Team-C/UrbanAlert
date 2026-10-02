using Application.Interfaces.Reportes;
using Infrastructure.Persistencia;
using Infrastructure.Persistencia.Repositorios;
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

        return services;
    }
}

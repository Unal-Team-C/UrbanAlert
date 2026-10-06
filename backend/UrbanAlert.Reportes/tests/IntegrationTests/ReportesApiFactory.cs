using Application.Geoespacial;
using Application.Interfaces.Geoespacial;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests;

public class ReportesApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Los tests de la API no dependen de que el servicio Geoespacial esté levantado.
        builder.ConfigureTestServices(services =>
            services.RemoveAll<IGeoespacialClient>().AddSingleton<IGeoespacialClient, GeoespacialFalso>());
    }

    private sealed class GeoespacialFalso : IGeoespacialClient
    {
        public Task<Guid> AsignarCoordenadaAsync(Guid idReporte, double latitud, double longitud, CancellationToken cancellationToken)
        {
            // Mismo rango de Bogotá que valida el servicio Geoespacial.
            if (latitud is < 3.72 or > 4.84 || longitud is < -74.46 or > -73.98)
                throw new CoordenadaInvalidaException("La coordenada no es válida: coordinate is outside the Bogotá range");

            return Task.FromResult(Guid.CreateVersion7());
        }
    }
}

using Domain.Reportes;

namespace Application.Interfaces.Reportes;

public interface IReporteRepository
{
    Task AgregarAsync(Reporte reporte, CancellationToken cancellationToken);

    Task<Reporte?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Reporte>> ObtenerTodosAsync(CancellationToken cancellationToken);

    Task ActualizarAsync(Reporte reporte, CancellationToken cancellationToken);

    Task EliminarAsync(Reporte reporte, CancellationToken cancellationToken);
}

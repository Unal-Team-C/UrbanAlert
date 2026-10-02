using Application.Reportes;

namespace Application.Interfaces.ObtenerReportes;

public interface IObtenerReportesHandler
{
    Task<IReadOnlyList<ReporteDto>> Handle(CancellationToken cancellationToken);
}

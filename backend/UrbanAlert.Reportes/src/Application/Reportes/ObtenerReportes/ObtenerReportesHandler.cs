using Application.Interfaces.ObtenerReportes;
using Application.Interfaces.Reportes;
using Domain.Reportes;

namespace Application.Reportes.ObtenerReportes;

public class ObtenerReportesHandler(IReporteRepository reporteRepository) : IObtenerReportesHandler
{
    public async Task<IReadOnlyList<ReporteDto>> Handle(CancellationToken cancellationToken)
    {
        IReadOnlyList<Reporte> reportes = await reporteRepository.ObtenerTodosAsync(cancellationToken);

        return reportes.Select(ReporteDto.DesdeEntidad).ToList();
    }
}

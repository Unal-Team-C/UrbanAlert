using Domain.Reportes;

namespace Application.Reportes.ObtenerReportes;

public record ObtenerReportesQuery(
    EstadoReporte? Estado,
    NivelEmergencia? NivelEmergencia,
    int Pagina,
    int TamanoPagina);

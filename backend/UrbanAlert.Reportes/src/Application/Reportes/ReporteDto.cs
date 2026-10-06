using Domain.Reportes;

namespace Application.Reportes;

public record ReporteDto(Guid Id,
    CategoriaReporte Categoria,
    TipoReporte Tipo,
    string Descripcion,
    Guid IdCoordenada,
    string UrlImagen,
    Guid IdUsuario,
    NivelEmergencia NivelEmergencia,
    EstadoReporte Estado,
    DateTime Fecha,
    Guid? IdResponsable,
    string? MotivoRechazo)
{
    public static ReporteDto DesdeEntidad(Reporte reporte) => new(
        reporte.Id,
        reporte.Categoria,
        reporte.Tipo,
        reporte.Descripcion,
        reporte.IdCoordenada,
        reporte.UrlImagen,
        reporte.IdUsuario,
        reporte.NivelEmergencia,
        reporte.Estado,
        reporte.Fecha,
        reporte.IdResponsable,
        reporte.MotivoRechazo);
}

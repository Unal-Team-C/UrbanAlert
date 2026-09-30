namespace Application.Reportes.CrearReporte;

public record CrearReporteCommand(
    string TipoDano,
    string Descripcion,
    Guid IdCoordenada,
    string UrlImagen,
    Guid IdUsuario
);
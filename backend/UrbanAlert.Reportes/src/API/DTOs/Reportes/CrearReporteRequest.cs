namespace API.DTOs.Reportes;

public record CrearReporteRequest(
    string TipoDano,
    string Descripcion,
    Guid IdCoordenada,
    string UrlImagen,
    Guid IdUsuario);


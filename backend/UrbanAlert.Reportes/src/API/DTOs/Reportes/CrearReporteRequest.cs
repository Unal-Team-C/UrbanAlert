using Domain.Reportes;

namespace API.DTOs.Reportes;

public record CrearReporteRequest(
    CategoriaReporte Categoria,
    TipoReporte Tipo,
    string Descripcion,
    Guid IdCoordenada,
    string UrlImagen);

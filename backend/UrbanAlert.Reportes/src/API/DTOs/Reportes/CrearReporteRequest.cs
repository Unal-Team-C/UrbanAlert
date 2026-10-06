using Domain.Reportes;

namespace API.DTOs.Reportes;

public record CrearReporteRequest(
    CategoriaDano Categoria,
    TipoDano TipoDano,
    string Descripcion,
    Guid IdCoordenada,
    string UrlImagen);

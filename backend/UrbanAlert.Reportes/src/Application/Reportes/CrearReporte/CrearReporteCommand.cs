using Domain.Reportes;

namespace Application.Reportes.CrearReporte;

public record CrearReporteCommand(
    CategoriaDano Categoria,
    TipoDano TipoDano,
    string Descripcion,
    Guid IdCoordenada,
    string UrlImagen
);

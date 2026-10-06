using Domain.Reportes;

namespace Application.Reportes.CrearReporte;

public record CrearReporteCommand(
    CategoriaReporte Categoria,
    TipoReporte Tipo,
    string Descripcion,
    double Latitud,
    double Longitud,
    string UrlImagen
);

using Domain.Reportes;

namespace API.DTOs.Reportes;

public record CrearReporteRequest(
    CategoriaDano Categoria,
    TipoDano TipoDano,
    string Descripcion,
    double Latitud,
    double Longitud,
    string UrlImagen);

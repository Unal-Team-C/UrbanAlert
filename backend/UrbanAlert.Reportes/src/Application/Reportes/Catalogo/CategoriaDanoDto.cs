using Domain.Reportes;

namespace Application.Reportes.Catalogo;

public record TipoDanoDto(TipoDano Codigo, string Nombre);

public record CategoriaDanoDto(CategoriaDano Codigo, string Nombre, IReadOnlyList<TipoDanoDto> TiposDano)
{
    public static IReadOnlyList<CategoriaDanoDto> DesdeCatalogo() =>
        CatalogoDanos.Categorias
            .Select(categoria => new CategoriaDanoDto(
                categoria.Categoria,
                categoria.Nombre,
                categoria.Tipos.Select(tipo => new TipoDanoDto(tipo.Tipo, tipo.Nombre)).ToList()))
            .ToList();
}

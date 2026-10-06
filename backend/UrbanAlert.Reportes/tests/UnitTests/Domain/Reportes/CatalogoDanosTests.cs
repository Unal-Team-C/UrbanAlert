using Domain.Reportes;

namespace UnitTests.Domain.Reportes;

public class CatalogoDanosTests
{
    [Fact]
    public void Catalogo_IncluyeCadaCategoriaUnaVez()
    {
        List<CategoriaDano> categorias = CatalogoDanos.Categorias.Select(categoria => categoria.Categoria).ToList();

        Assert.Equal(Enum.GetValues<CategoriaDano>().Order(), categorias.Order());
    }

    [Fact]
    public void Catalogo_IncluyeCadaTipoDeDanoUnaVez()
    {
        List<TipoDano> tipos = CatalogoDanos.Categorias.SelectMany(categoria => categoria.Tipos).Select(tipo => tipo.Tipo).ToList();

        Assert.Equal(Enum.GetValues<TipoDano>().Order(), tipos.Order());
    }

    [Fact]
    public void Catalogo_TieneLasDiezCategoriasYCincuentaYUnTipos()
    {
        Assert.Equal(10, CatalogoDanos.Categorias.Count);
        Assert.Equal(51, CatalogoDanos.Categorias.Sum(categoria => categoria.Tipos.Count));
    }

    [Fact]
    public void Catalogo_RespetaLaConvencionDeValoresCategoriaPorCien()
    {
        foreach (CategoriaDanoCatalogo categoria in CatalogoDanos.Categorias)
        foreach (TipoDanoCatalogo tipo in categoria.Tipos)
            Assert.Equal((int)categoria.Categoria, (int)tipo.Tipo / 100);
    }

    [Theory]
    [InlineData(TipoDano.HuecosEnLaVia, CategoriaDano.ViasYAndenes, true)]
    [InlineData(TipoDano.Inundacion, CategoriaDano.AguaYAlcantarillado, true)]
    [InlineData(TipoDano.Otros, CategoriaDano.Otros, true)]
    [InlineData(TipoDano.HuecosEnLaVia, CategoriaDano.Aseo, false)]
    [InlineData((TipoDano)99, CategoriaDano.ViasYAndenes, false)]
    public void PerteneceA_ValidaLaCategoriaDelTipo(TipoDano tipo, CategoriaDano categoria, bool esperado)
    {
        Assert.Equal(esperado, CatalogoDanos.PerteneceA(tipo, categoria));
    }
}

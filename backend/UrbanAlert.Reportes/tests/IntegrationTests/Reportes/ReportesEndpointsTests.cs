using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using API.DTOs.Reportes;
using Application.Comun;
using Domain.Reportes;

namespace IntegrationTests.Reportes;

public class ReportesEndpointsTests : IClassFixture<ReportesApiFactory>
{
    // Mismas reglas que la API: enums como códigos UPPER_SNAKE_CASE, sin números.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(CodigoEnum.Politica, allowIntegerValues: false) }
    };

    private readonly HttpClient _client;

    public ReportesEndpointsTests(ReportesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static CrearReporteRequest ReporteDePrueba() => new(
        CategoriaReporte.ViasYAndenes,
        TipoReporte.HuecosEnLaVia,
        "Hueco grande que afecta el tránsito vehicular",
        Guid.NewGuid(),
        "https://imagenes.urbanalert.com/foto.jpg");

    private async Task<Guid> CrearReporteAsync()
    {
        HttpResponseMessage respuesta = await _client.PostAsJsonAsync("/api/v1/Reportes", ReporteDePrueba(), Json);
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);

        CrearReporteRespuesta? creado = await respuesta.Content.ReadFromJsonAsync<CrearReporteRespuesta>();
        Assert.NotNull(creado);

        return creado!.IdReporte;
    }

    [Fact]
    public async Task CrearYObtenerReporte_PersisteEnPostgresConValoresPorDefecto()
    {
        Guid idReporte = await CrearReporteAsync();

        HttpResponseMessage respuestaConsulta = await _client.GetAsync($"/api/v1/Reportes/{idReporte}");
        Assert.Equal(HttpStatusCode.OK, respuestaConsulta.StatusCode);

        ReporteRespuesta? reporte = await respuestaConsulta.Content.ReadFromJsonAsync<ReporteRespuesta>();
        Assert.NotNull(reporte);
        Assert.Equal(idReporte, reporte!.Id);
        Assert.Equal("VIAS_Y_ANDENES", reporte.Categoria);
        Assert.Equal("HUECOS_EN_LA_VIA", reporte.Tipo);
        Assert.Equal("DEFAULT", reporte.NivelEmergencia);
        Assert.Equal("REPORTADO", reporte.Estado);
        Assert.Null(reporte.IdResponsable);
        Assert.Null(reporte.MotivoRechazo);
    }

    [Fact]
    public async Task ObtenerReportes_IncluyeLosReportesCreados()
    {
        Guid idReporte = await CrearReporteAsync();

        HttpResponseMessage respuesta = await _client.GetAsync("/api/v1/Reportes");
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        PaginaRespuesta? pagina = await respuesta.Content.ReadFromJsonAsync<PaginaRespuesta>();
        Assert.NotNull(pagina);
        Assert.Contains(pagina!.Elementos, r => r.Id == idReporte);
    }

    [Fact]
    public async Task ObtenerReportes_FiltraPorEstado()
    {
        Guid idReporte = await CrearReporteAsync();

        HttpResponseMessage respuesta = await _client.GetAsync("/api/v1/Reportes?estado=RECHAZADO");
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        PaginaRespuesta? pagina = await respuesta.Content.ReadFromJsonAsync<PaginaRespuesta>();
        Assert.NotNull(pagina);
        Assert.DoesNotContain(pagina!.Elementos, r => r.Id == idReporte);
    }

    [Fact]
    public async Task ObtenerReportes_RespetaElTamanoDePaginaSolicitado()
    {
        await CrearReporteAsync();
        await CrearReporteAsync();

        HttpResponseMessage respuesta = await _client.GetAsync("/api/v1/Reportes?pagina=1&tamanoPagina=1");
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        PaginaRespuesta? pagina = await respuesta.Content.ReadFromJsonAsync<PaginaRespuesta>();
        Assert.NotNull(pagina);
        Assert.Single(pagina!.Elementos);
        Assert.True(pagina.TotalElementos >= 2);
    }

    [Fact]
    public async Task ActualizarEstado_AvanzaLaMaquinaDeEstadosYPersisteElCambio()
    {
        Guid idReporte = await CrearReporteAsync();

        HttpResponseMessage respuestaActualizacion = await _client.PatchAsJsonAsync(
            $"/api/v1/Reportes/{idReporte}/estado", new ActualizarEstadoRequest(EstadoReporte.Verificado), Json);
        Assert.Equal(HttpStatusCode.OK, respuestaActualizacion.StatusCode);

        HttpResponseMessage respuestaConsulta = await _client.GetAsync($"/api/v1/Reportes/{idReporte}");
        ReporteRespuesta? reporte = await respuestaConsulta.Content.ReadFromJsonAsync<ReporteRespuesta>();

        Assert.Equal("VERIFICADO", reporte!.Estado);
    }

    [Fact]
    public async Task ActualizarEstado_RechazaUnSaltoDeEstadoConBadRequest()
    {
        Guid idReporte = await CrearReporteAsync();

        HttpResponseMessage respuesta = await _client.PatchAsJsonAsync(
            $"/api/v1/Reportes/{idReporte}/estado", new ActualizarEstadoRequest(EstadoReporte.Asignado), Json);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task ActualizarNivelEmergencia_PersisteElNuevoNivel()
    {
        Guid idReporte = await CrearReporteAsync();

        HttpResponseMessage respuesta = await _client.PatchAsJsonAsync(
            $"/api/v1/Reportes/{idReporte}/nivel-emergencia", new ActualizarNivelEmergenciaRequest(NivelEmergencia.Alta), Json);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        HttpResponseMessage respuestaConsulta = await _client.GetAsync($"/api/v1/Reportes/{idReporte}");
        ReporteRespuesta? reporte = await respuestaConsulta.Content.ReadFromJsonAsync<ReporteRespuesta>();

        Assert.Equal("ALTA", reporte!.NivelEmergencia);
    }

    [Fact]
    public async Task AsignarResponsable_PersisteElResponsableSinCambiarElEstado()
    {
        Guid idReporte = await CrearReporteAsync();
        Guid idResponsable = Guid.NewGuid();

        HttpResponseMessage respuesta = await _client.PatchAsJsonAsync(
            $"/api/v1/Reportes/{idReporte}/asignacion", new AsignarResponsableRequest(idResponsable));
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        HttpResponseMessage respuestaConsulta = await _client.GetAsync($"/api/v1/Reportes/{idReporte}");
        ReporteRespuesta? reporte = await respuestaConsulta.Content.ReadFromJsonAsync<ReporteRespuesta>();

        Assert.Equal(idResponsable, reporte!.IdResponsable);
        Assert.Equal("REPORTADO", reporte.Estado);
    }

    [Fact]
    public async Task RechazarReporte_PersisteElEstadoRechazadoYElMotivo()
    {
        Guid idReporte = await CrearReporteAsync();

        HttpResponseMessage respuesta = await _client.PutAsJsonAsync(
            $"/api/v1/Reportes/{idReporte}/rechazo", new RechazarReporteRequest("Reporte duplicado"));
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        HttpResponseMessage respuestaConsulta = await _client.GetAsync($"/api/v1/Reportes/{idReporte}");
        ReporteRespuesta? reporte = await respuestaConsulta.Content.ReadFromJsonAsync<ReporteRespuesta>();

        Assert.Equal("RECHAZADO", reporte!.Estado);
        Assert.Equal("Reporte duplicado", reporte.MotivoRechazo);
    }

    [Fact]
    public async Task EliminarReporte_LoBorraDeLaBaseDeDatos()
    {
        Guid idReporte = await CrearReporteAsync();

        HttpResponseMessage respuestaEliminacion = await _client.DeleteAsync($"/api/v1/Reportes/{idReporte}");
        Assert.Equal(HttpStatusCode.NoContent, respuestaEliminacion.StatusCode);

        HttpResponseMessage respuestaConsulta = await _client.GetAsync($"/api/v1/Reportes/{idReporte}");
        Assert.Equal(HttpStatusCode.NotFound, respuestaConsulta.StatusCode);
    }

    [Fact]
    public async Task ObtenerReporteInexistente_Retorna404()
    {
        HttpResponseMessage respuesta = await _client.GetAsync($"/api/v1/Reportes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task CrearReporte_AceptaLosCodigosDelCatalogoComoTexto()
    {
        HttpResponseMessage respuesta = await PostJsonAsync("""
            {"categoria":"SENALIZACION","tipo":"SEMAFORO_APAGADO","descripcion":"Semáforo sin luz",
             "idCoordenada":"7d4a3c1e-2b5f-4e8a-9c0d-1f2e3a4b5c6d","urlImagen":"https://imagenes.urbanalert.com/foto.jpg"}
            """);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
    }

    [Fact]
    public async Task CrearReporte_Devuelve400_SiElTipoNoPerteneceALaCategoria()
    {
        HttpResponseMessage respuesta = await _client.PostAsJsonAsync(
            "/api/v1/Reportes", ReporteDePrueba() with { Categoria = CategoriaReporte.Aseo }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task CrearReporte_Devuelve400_SiElTipoNoExisteEnElCatalogo()
    {
        HttpResponseMessage respuesta = await PostJsonAsync("""
            {"categoria":"VIAS_Y_ANDENES","tipo":"BACHE","descripcion":"Bache",
             "idCoordenada":"7d4a3c1e-2b5f-4e8a-9c0d-1f2e3a4b5c6d","urlImagen":"https://imagenes.urbanalert.com/foto.jpg"}
            """);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task ObtenerCatalogo_DevuelveLasCategoriasConSusTipos()
    {
        List<CategoriaRespuesta>? catalogo = await _client.GetFromJsonAsync<List<CategoriaRespuesta>>("/api/v1/Reportes/catalogo");

        Assert.NotNull(catalogo);
        Assert.Equal(10, catalogo!.Count);
        CategoriaRespuesta vias = catalogo[0];
        Assert.Equal("VIAS_Y_ANDENES", vias.Codigo);
        Assert.Equal("Vías y andenes", vias.Nombre);
        Assert.Contains(vias.Tipos, tipo => tipo.Codigo == "HUECOS_EN_LA_VIA" && tipo.Nombre == "Huecos en la vía");
        Assert.Equal(51, catalogo.Sum(categoria => categoria.Tipos.Count));
    }

    [Theory]
    [InlineData("\"categoria\":1,\"tipo\":101")]
    [InlineData("\"categoria\":\"ViasYAndenes\",\"tipo\":\"HuecosEnLaVia\"")]
    public async Task CrearReporte_Devuelve400_SiLosCodigosNoSonUpperSnakeCase(string codigos)
    {
        HttpResponseMessage respuesta = await PostJsonAsync($$"""
            {{{codigos}}},"descripcion":"Hueco",
             "idCoordenada":"7d4a3c1e-2b5f-4e8a-9c0d-1f2e3a4b5c6d","urlImagen":"https://imagenes.urbanalert.com/foto.jpg"}
            """);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Theory]
    [InlineData("Rechazado")]
    [InlineData("5")]
    [InlineData("NO_EXISTE")]
    public async Task ObtenerReportes_Devuelve400_SiElFiltroNoEsUnCodigoValido(string estado)
    {
        HttpResponseMessage respuesta = await _client.GetAsync($"/api/v1/Reportes?estado={estado}");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    private Task<HttpResponseMessage> PostJsonAsync(string json) =>
        _client.PostAsync("/api/v1/Reportes", new StringContent(json, Encoding.UTF8, "application/json"));

    private record CategoriaRespuesta(string Codigo, string Nombre, List<TipoRespuesta> Tipos);

    private record TipoRespuesta(string Codigo, string Nombre);

    private record CrearReporteRespuesta(Guid IdReporte, string Message);

    private record PaginaRespuesta(
        List<ReporteRespuesta> Elementos,
        int Pagina,
        int TamanoPagina,
        int TotalElementos);

    private record ReporteRespuesta(
        Guid Id,
        string Categoria,
        string Tipo,
        string Descripcion,
        Guid IdCoordenada,
        string UrlImagen,
        Guid IdUsuario,
        string NivelEmergencia,
        string Estado,
        DateTime Fecha,
        Guid? IdResponsable,
        string? MotivoRechazo);
}

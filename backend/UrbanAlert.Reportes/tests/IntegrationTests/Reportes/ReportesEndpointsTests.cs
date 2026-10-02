using System.Net;
using System.Net.Http.Json;
using API.DTOs.Reportes;

namespace IntegrationTests.Reportes;

public class ReportesEndpointsTests : IClassFixture<ReportesApiFactory>
{
    private readonly HttpClient _client;

    public ReportesEndpointsTests(ReportesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static CrearReporteRequest ReporteDePrueba() => new(
        "Hueco en la vía",
        "Hueco grande que afecta el tránsito vehicular",
        Guid.NewGuid(),
        "https://imagenes.urbanalert.com/foto.jpg");

    private async Task<Guid> CrearReporteAsync()
    {
        HttpResponseMessage respuesta = await _client.PostAsJsonAsync("/api/v1/Reportes", ReporteDePrueba());
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
        Assert.Equal("Default", reporte.NivelEmergencia);
        Assert.Equal("Reportado", reporte.Estado);
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

        HttpResponseMessage respuesta = await _client.GetAsync("/api/v1/Reportes?estado=Rechazado");
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
            $"/api/v1/Reportes/{idReporte}/estado", new ActualizarEstadoRequest("Verificado"));
        Assert.Equal(HttpStatusCode.OK, respuestaActualizacion.StatusCode);

        HttpResponseMessage respuestaConsulta = await _client.GetAsync($"/api/v1/Reportes/{idReporte}");
        ReporteRespuesta? reporte = await respuestaConsulta.Content.ReadFromJsonAsync<ReporteRespuesta>();

        Assert.Equal("Verificado", reporte!.Estado);
    }

    [Fact]
    public async Task ActualizarEstado_RechazaUnSaltoDeEstadoConBadRequest()
    {
        Guid idReporte = await CrearReporteAsync();

        HttpResponseMessage respuesta = await _client.PatchAsJsonAsync(
            $"/api/v1/Reportes/{idReporte}/estado", new ActualizarEstadoRequest("Asignado"));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task ActualizarNivelEmergencia_PersisteElNuevoNivel()
    {
        Guid idReporte = await CrearReporteAsync();

        HttpResponseMessage respuesta = await _client.PatchAsJsonAsync(
            $"/api/v1/Reportes/{idReporte}/nivel-emergencia", new ActualizarNivelEmergenciaRequest(3));
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        HttpResponseMessage respuestaConsulta = await _client.GetAsync($"/api/v1/Reportes/{idReporte}");
        ReporteRespuesta? reporte = await respuestaConsulta.Content.ReadFromJsonAsync<ReporteRespuesta>();

        Assert.Equal("Alta", reporte!.NivelEmergencia);
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
        Assert.Equal("Reportado", reporte.Estado);
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

        Assert.Equal("Rechazado", reporte!.Estado);
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

    private record CrearReporteRespuesta(Guid IdReporte, string Message);

    private record PaginaRespuesta(
        List<ReporteRespuesta> Elementos,
        int Pagina,
        int TamanoPagina,
        int TotalElementos);

    private record ReporteRespuesta(
        Guid Id,
        string TipoDano,
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

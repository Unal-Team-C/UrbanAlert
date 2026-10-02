using Domain.Reportes;

namespace UnitTests.Domain.Reportes;

public class ReporteTests
{
    private static Reporte CrearReporteValido() => new(
        "Hueco en la vía",
        "Hueco grande que afecta el tránsito vehicular",
        Guid.NewGuid(),
        "https://imagenes.urbanalert.com/foto.jpg",
        Guid.NewGuid());

    [Fact]
    public void Constructor_AsignaValoresPorDefecto()
    {
        DateTime antes = DateTime.UtcNow;

        Reporte reporte = CrearReporteValido();

        DateTime despues = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, reporte.Id);
        Assert.Equal(7, reporte.Id.Version);
        Assert.Equal(NivelEmergencia.Default, reporte.NivelEmergencia);
        Assert.Equal(EstadoReporte.Reportado, reporte.Estado);
        Assert.Null(reporte.IdResponsable);
        Assert.Null(reporte.MotivoRechazo);
        Assert.InRange(reporte.Fecha, antes, despues);
    }

    [Theory]
    [InlineData("", "descripcion", "url")]
    [InlineData("tipo", "", "url")]
    [InlineData("tipo", "descripcion", "")]
    public void Constructor_LanzaExcepcion_SiCamposDeTextoObligatoriosEstanVacios(string tipoDano, string descripcion, string urlImagen)
    {
        Assert.Throws<ArgumentException>(() => new Reporte(tipoDano, descripcion, Guid.NewGuid(), urlImagen, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_LanzaExcepcion_SiIdCoordenadaEsVacio()
    {
        Assert.Throws<ArgumentException>(() => new Reporte("tipo", "descripcion", Guid.Empty, "url", Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_LanzaExcepcion_SiIdUsuarioEsVacio()
    {
        Assert.Throws<ArgumentException>(() => new Reporte("tipo", "descripcion", Guid.NewGuid(), "url", Guid.Empty));
    }

    [Theory]
    [InlineData(EstadoReporte.Reportado, EstadoReporte.Verificado)]
    [InlineData(EstadoReporte.Verificado, EstadoReporte.Asignado)]
    [InlineData(EstadoReporte.Asignado, EstadoReporte.EnIntervencion)]
    [InlineData(EstadoReporte.EnIntervencion, EstadoReporte.Resuelto)]
    public void ActualizarEstado_PermiteLaTransicionLinealSiguiente(EstadoReporte estadoInicial, EstadoReporte estadoSiguiente)
    {
        Reporte reporte = CrearReporteValido();
        AvanzarHasta(reporte, estadoInicial);

        reporte.ActualizarEstado(estadoSiguiente);

        Assert.Equal(estadoSiguiente, reporte.Estado);
    }

    [Fact]
    public void ActualizarEstado_LanzaExcepcion_SiSeIntentaSaltarUnEstado()
    {
        Reporte reporte = CrearReporteValido();

        Assert.Throws<TransicionEstadoInvalidaException>(() => reporte.ActualizarEstado(EstadoReporte.Asignado));
    }

    [Fact]
    public void ActualizarEstado_LanzaExcepcion_SiElReporteYaEstaResuelto()
    {
        Reporte reporte = CrearReporteValido();
        AvanzarHasta(reporte, EstadoReporte.Resuelto);

        Assert.Throws<TransicionEstadoInvalidaException>(() => reporte.ActualizarEstado(EstadoReporte.Resuelto));
    }

    [Theory]
    [InlineData(EstadoReporte.Reportado)]
    [InlineData(EstadoReporte.Verificado)]
    [InlineData(EstadoReporte.Asignado)]
    [InlineData(EstadoReporte.EnIntervencion)]
    public void Rechazar_CambiaEstadoYRegistraMotivo_DesdeCualquierEstadoPrevioAResuelto(EstadoReporte estadoInicial)
    {
        Reporte reporte = CrearReporteValido();
        AvanzarHasta(reporte, estadoInicial);

        reporte.Rechazar("Reporte duplicado");

        Assert.Equal(EstadoReporte.Rechazado, reporte.Estado);
        Assert.Equal("Reporte duplicado", reporte.MotivoRechazo);
    }

    [Fact]
    public void Rechazar_LanzaExcepcion_SiElReporteYaEstaResuelto()
    {
        Reporte reporte = CrearReporteValido();
        AvanzarHasta(reporte, EstadoReporte.Resuelto);

        Assert.Throws<TransicionEstadoInvalidaException>(() => reporte.Rechazar("motivo"));
    }

    [Fact]
    public void Rechazar_LanzaExcepcion_SiElMotivoEsVacio()
    {
        Reporte reporte = CrearReporteValido();

        Assert.Throws<ArgumentException>(() => reporte.Rechazar(""));
    }

    [Fact]
    public void AsignarResponsable_EstableceElResponsable_SinCambiarElEstado()
    {
        Reporte reporte = CrearReporteValido();
        Guid idResponsable = Guid.NewGuid();

        reporte.AsignarResponsable(idResponsable);

        Assert.Equal(idResponsable, reporte.IdResponsable);
        Assert.Equal(EstadoReporte.Reportado, reporte.Estado);
    }

    [Fact]
    public void ActualizarNivelEmergencia_CambiaElNivel()
    {
        Reporte reporte = CrearReporteValido();

        reporte.ActualizarNivelEmergencia(NivelEmergencia.Alta);

        Assert.Equal(NivelEmergencia.Alta, reporte.NivelEmergencia);
    }

    private static void AvanzarHasta(Reporte reporte, EstadoReporte estadoObjetivo)
    {
        EstadoReporte[] secuencia =
        [
            EstadoReporte.Verificado,
            EstadoReporte.Asignado,
            EstadoReporte.EnIntervencion,
            EstadoReporte.Resuelto
        ];

        foreach (EstadoReporte estado in secuencia)
        {
            if (reporte.Estado == estadoObjetivo)
            {
                return;
            }

            reporte.ActualizarEstado(estado);
        }
    }
}

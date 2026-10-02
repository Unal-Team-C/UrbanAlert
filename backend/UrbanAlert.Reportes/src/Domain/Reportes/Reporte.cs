namespace Domain.Reportes;

public class Reporte
{
    private static readonly Dictionary<EstadoReporte, EstadoReporte> SiguienteEstado = new()
    {
        [EstadoReporte.Reportado] = EstadoReporte.Verificado,
        [EstadoReporte.Verificado] = EstadoReporte.Asignado,
        [EstadoReporte.Asignado] = EstadoReporte.EnIntervencion,
        [EstadoReporte.EnIntervencion] = EstadoReporte.Resuelto
    };

    public Guid Id { get; private set; }
    public string TipoDano { get; private set; } = null!;
    public string Descripcion { get; private set; } = null!;
    public Guid IdCoordenada { get; private set; }
    public string UrlImagen { get; private set; } = null!;
    public Guid IdUsuario { get; private set; }
    public NivelEmergencia NivelEmergencia { get; private set; }
    public EstadoReporte Estado { get; private set; }
    public DateTime Fecha { get; private set; }
    public Guid? IdResponsable { get; private set; }
    public string? MotivoRechazo { get; private set; }

    private Reporte() { }

    public Reporte(string tipoDano,
        string descripcion,
        Guid idCoordenada,
        string urlImagen,
        Guid idUsuario)
    {
        if (string.IsNullOrWhiteSpace(tipoDano))
            throw new ArgumentException("El tipo de daño es obligatorio.", nameof(tipoDano));

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException("La descripción es obligatoria.", nameof(descripcion));

        if (idCoordenada == Guid.Empty)
            throw new ArgumentException("El identificador de la coordenada es obligatorio.", nameof(idCoordenada));

        if (string.IsNullOrWhiteSpace(urlImagen))
            throw new ArgumentException("La URL de la imagen es obligatoria.", nameof(urlImagen));

        if (idUsuario == Guid.Empty)
            throw new ArgumentException("El identificador del usuario es obligatorio.", nameof(idUsuario));

        Id = Guid.CreateVersion7();
        TipoDano = tipoDano;
        Descripcion = descripcion;
        IdCoordenada = idCoordenada;
        UrlImagen = urlImagen;
        IdUsuario = idUsuario;
        NivelEmergencia = NivelEmergencia.Default;
        Estado = EstadoReporte.Reportado;
        Fecha = DateTime.UtcNow;
    }

    public void ActualizarEstado(EstadoReporte nuevoEstado)
    {
        if (!SiguienteEstado.TryGetValue(Estado, out EstadoReporte estadoEsperado) || estadoEsperado != nuevoEstado)
            throw new TransicionEstadoInvalidaException(Estado, nuevoEstado);

        Estado = nuevoEstado;
    }

    public void AsignarResponsable(Guid idResponsable)
    {
        if (idResponsable == Guid.Empty)
            throw new ArgumentException("El identificador del responsable es obligatorio.", nameof(idResponsable));

        IdResponsable = idResponsable;
    }

    public void ActualizarNivelEmergencia(NivelEmergencia nivelEmergencia)
    {
        NivelEmergencia = nivelEmergencia;
    }

    public void Rechazar(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("El motivo de rechazo es obligatorio.", nameof(motivo));

        if (Estado is EstadoReporte.Resuelto or EstadoReporte.Rechazado)
            throw new TransicionEstadoInvalidaException(Estado, EstadoReporte.Rechazado);

        Estado = EstadoReporte.Rechazado;
        MotivoRechazo = motivo;
    }
}

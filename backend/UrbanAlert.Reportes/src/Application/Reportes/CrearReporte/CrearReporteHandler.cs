using Application.Interfaces.CrearReporte;
using Application.Interfaces.Eventos;
using Application.Interfaces.Geoespacial;
using Application.Interfaces.Reportes;
using Application.Reportes.Eventos;
using Domain.Reportes;

namespace Application.Reportes.CrearReporte;

public class CrearReporteHandler : ICrearReporteHandler
{
    private readonly IReporteRepository _reporteRepository;
    private readonly IGeoespacialClient _geoespacialClient;
    private readonly IEventPublisher _eventPublisher;

    public CrearReporteHandler(
        IReporteRepository reporteRepository,
        IGeoespacialClient geoespacialClient,
        IEventPublisher eventPublisher)
    {
        _reporteRepository = reporteRepository;
        _geoespacialClient = geoespacialClient;
        _eventPublisher = eventPublisher;
    }

    public async Task<Guid> Handle(CrearReporteCommand command, CancellationToken cancellationToken)
    {
        // Sin autenticación todavía: se asigna un usuario genérico hasta que
        // exista un proveedor de identidad real del cual tomar el usuario actual.
        Guid idUsuarioGenerico = Guid.NewGuid();

        // Se valida el reporte antes de llamar a Geoespacial para no registrar
        // coordenadas de reportes que nunca se van a crear.
        Reporte reporte = new Reporte(
            command.Categoria,
            command.Tipo,
            command.Descripcion,
            command.UrlImagen,
            idUsuarioGenerico);

        Guid idCoordenada = await _geoespacialClient.AsignarCoordenadaAsync(
            reporte.Id, command.Latitud, command.Longitud, cancellationToken);

        reporte.AsignarCoordenada(idCoordenada);

        await _reporteRepository.AgregarAsync(reporte, cancellationToken);

        ReporteCreadoEvent evento = new(
            Guid.CreateVersion7(),
            new ReporteEventoDto(reporte.Id, reporte.Estado, reporte.IdCoordenada, reporte.IdUsuario, reporte.Fecha));

        await _eventPublisher.PublicarAsync(evento, cancellationToken);

        return reporte.Id;
    }
}

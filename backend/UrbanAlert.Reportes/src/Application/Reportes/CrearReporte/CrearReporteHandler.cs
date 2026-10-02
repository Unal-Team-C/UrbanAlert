using Application.Interfaces.CrearReporte;
using Application.Interfaces.Eventos;
using Application.Interfaces.Reportes;
using Application.Reportes.Eventos;
using Domain.Reportes;

namespace Application.Reportes.CrearReporte;

public class CrearReporteHandler : ICrearReporteHandler
{
    private readonly IReporteRepository _reporteRepository;
    private readonly IEventPublisher _eventPublisher;

    public CrearReporteHandler(IReporteRepository reporteRepository, IEventPublisher eventPublisher)
    {
        _reporteRepository = reporteRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Guid> Handle(CrearReporteCommand command, CancellationToken cancellationToken)
    {
        // Sin autenticación todavía: se asigna un usuario genérico hasta que
        // exista un proveedor de identidad real del cual tomar el usuario actual.
        Guid idUsuarioGenerico = Guid.NewGuid();

        Reporte reporte = new Reporte(
            command.TipoDano,
            command.Descripcion,
            command.IdCoordenada,
            command.UrlImagen,
            idUsuarioGenerico);

        await _reporteRepository.AgregarAsync(reporte, cancellationToken);

        ReporteCreadoEvent evento = new(
            Guid.CreateVersion7(),
            new ReporteEventoDto(reporte.Id, reporte.Estado, reporte.IdCoordenada, reporte.IdUsuario, reporte.Fecha));

        await _eventPublisher.PublicarAsync(evento, cancellationToken);

        return reporte.Id;
    }
}

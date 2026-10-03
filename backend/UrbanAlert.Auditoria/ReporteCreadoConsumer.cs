using Application.Reportes.Eventos;
using MassTransit;

namespace UrbanAlert.Auditoria;

public sealed class ReporteCreadoConsumer(
    IAuditStore auditStore,
    IAuditArchive auditArchive,
    ILogger<ReporteCreadoConsumer> logger) : IConsumer<ReporteCreadoEvent>
{
    public async Task Consume(ConsumeContext<ReporteCreadoEvent> context)
    {
        AuditEvent auditEvent = DotNetAuditEventFactory.FromReporteCreado(context.Message, context.CorrelationId);
        AuditEventResponse persisted = await auditStore.PersistAsync(auditEvent, context.CancellationToken);
        await auditArchive.ArchiveAsync(persisted, context.CancellationToken);
        logger.LogInformation("Evento .NET de creación auditado. EventId={EventId} ReportId={ReportId}",
            auditEvent.EventId, auditEvent.ReportId);
    }
}

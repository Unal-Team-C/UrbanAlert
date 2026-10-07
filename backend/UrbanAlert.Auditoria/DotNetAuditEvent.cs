using System.Text.Json;
using Application.Reportes.Eventos;

namespace UrbanAlert.Auditoria;

public sealed record AuditEvent(
    Guid EventId,
    string EventType,
    int Version,
    DateTimeOffset OccurredAt,
    Guid? CorrelationId,
    Guid? ReportId,
    Guid ActorId,
    JsonElement Data,
    JsonElement Payload);

public static class DotNetAuditEventFactory
{
    public static AuditEvent FromReporteCreado(ReporteCreadoEvent message, Guid? correlationId)
    {
        DateTime occurredAtUtc = message.Reporte.Fecha.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(message.Reporte.Fecha, DateTimeKind.Utc)
            : message.Reporte.Fecha.ToUniversalTime();
        DateTimeOffset occurredAt = new(occurredAtUtc);
        object data = new
        {
            actorId = message.Reporte.IdUsuario,
            idCoordenada = message.Reporte.IdCoordenada,
            estado = CodigosEnum.ACodigo(message.Reporte.Estado)
        };
        object payload = new
        {
            eventId = message.IdEvento,
            eventType = "reporte.creado",
            version = 1,
            occurredAt,
            correlationId,
            reportId = message.Reporte.Id,
            data
        };

        using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(payload));
        JsonElement root = document.RootElement;
        JsonElement eventData = root.GetProperty("data").Clone();
        return new AuditEvent(message.IdEvento, "reporte.creado", 1, occurredAt, correlationId,
            message.Reporte.Id, message.Reporte.IdUsuario, eventData, root.Clone());
    }
}

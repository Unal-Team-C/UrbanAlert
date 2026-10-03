namespace UrbanAlert.Auditoria;

public interface IAuditStore
{
    Task<AuditEventResponse> PersistAsync(AuditEvent auditEvent, CancellationToken cancellationToken);
    Task<AuditTimeline> GetTimelineAsync(Guid reportId, long cursor, int limit, CancellationToken cancellationToken);
    Task<AuditIntegrityReport> VerifyAsync(Guid reportId, CancellationToken cancellationToken);
}

public interface IAuditAuthorizationService
{
    Task<AuditUserContext> ResolveUserAsync(System.Security.Claims.ClaimsPrincipal principal, CancellationToken cancellationToken);
    Task<int?> CheckReportAccessAsync(AuditUserContext user, Guid reportId, CancellationToken cancellationToken);
}

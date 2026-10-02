using Application.Interfaces.Reportes;
using Domain.Reportes;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistencia.Repositorios;

public class ReporteRepository(ReportesDbContext dbContext) : IReporteRepository
{
    public async Task AgregarAsync(Reporte reporte, CancellationToken cancellationToken)
    {
        await dbContext.Reportes.AddAsync(reporte, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Reporte?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Reportes.FirstOrDefaultAsync(reporte => reporte.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Reporte>> ObtenerTodosAsync(CancellationToken cancellationToken) =>
        await dbContext.Reportes
            .AsNoTracking()
            .OrderByDescending(reporte => reporte.Fecha)
            .ToListAsync(cancellationToken);

    public Task ActualizarAsync(Reporte reporte, CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public Task EliminarAsync(Reporte reporte, CancellationToken cancellationToken)
    {
        dbContext.Reportes.Remove(reporte);
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

using Application.Interfaces.CrearReporte;
using Application.Interfaces.Reportes;
using Domain.Reportes;

namespace Application.Reportes.CrearReporte;

public class CrearReporteHandler : ICrearReporteHandler
{
    private readonly IReporteRepository _reporteRepository;

    public CrearReporteHandler(IReporteRepository reporteRepository)
    {
        _reporteRepository = reporteRepository;
    }

    public async Task<Guid> Handle(CrearReporteCommand command, CancellationToken cancellationToken)
    {
        Reporte reporte = new Reporte(
            command.TipoDano,
            command.Descripcion,
            command.IdCoordenada,
            command.UrlImagen,
            command.IdUsuario);

        await _reporteRepository.AgregarAsync(reporte, cancellationToken);

        return reporte.Id;
    }
}

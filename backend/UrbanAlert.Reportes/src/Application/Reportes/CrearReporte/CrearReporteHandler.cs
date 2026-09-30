using Application.Interfaces.CrearReporte;

namespace Application.Reportes.CrearReporte;

public class CrearReporteHandler : ICrearReporteHandler
{
    public async Task<Guid> Handle(CrearReporteCommand command, CancellationToken cancellationToken)
    {
        // Validar reglas de aplicación
        // Obtener/verificar usuario
        // Obtener/verificar coordenada
        // Crear entidad Reporte
        // Guardar
        // Retornar IdReporte

        throw new NotImplementedException();
    }
}
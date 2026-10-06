using Application.Imagenes;
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

        string? nombreImagen = null;
        if (command.Imagen is not null)
        {
            // Reportes recibe el archivo junto con el reporte. Por ahora se valida que sea una
            // imagen y solo se guarda su nombre; más adelante el servicio de Multimedia vinculará
            // la imagen y devolverá la ruta alojada, que se guardará como UrlImagen.
            await ValidadorImagen.ValidarAsync(command.Imagen.Contenido, command.Imagen.Tamano, cancellationToken);
            nombreImagen = NombreDeArchivo(command.Imagen.NombreArchivo);
        }

        // Se valida el reporte antes de llamar a Geoespacial para no registrar
        // coordenadas de reportes que nunca se van a crear.
        Reporte reporte = new Reporte(
            command.Categoria,
            command.Tipo,
            command.Descripcion,
            command.UrlImagen,
            nombreImagen,
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

    // Algunos navegadores envían la ruta completa ("C:\fakepath\foto.jpg"): solo interesa el
    // nombre. Si es muy largo se recorta conservando la extensión.
    private static string NombreDeArchivo(string nombreEnviado)
    {
        string nombre = nombreEnviado.Split('/', '\\').Last().Trim();
        if (nombre.Length == 0 || nombre is "." or "..")
            return "imagen";

        if (nombre.Length <= Reporte.NombreImagenMaxLength)
            return nombre;

        string extension = Path.GetExtension(nombre);
        if (extension.Length > 10)
            extension = "";

        return nombre[..(Reporte.NombreImagenMaxLength - extension.Length)] + extension;
    }
}

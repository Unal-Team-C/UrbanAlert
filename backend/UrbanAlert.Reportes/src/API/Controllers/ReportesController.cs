using Application.Interfaces.ActualizarEstadoReporte;
using Application.Interfaces.ActualizarNivelEmergenciaReporte;
using Application.Interfaces.AsignarResponsableReporte;
using Application.Interfaces.CrearReporte;
using Application.Interfaces.EliminarReporte;
using Application.Interfaces.ObtenerReportePorId;
using Application.Interfaces.ObtenerReportes;
using Application.Interfaces.RechazarReporte;
using Application.Reportes.ActualizarEstadoReporte;
using Application.Reportes.ActualizarNivelEmergenciaReporte;
using Application.Reportes.AsignarResponsableReporte;
using Application.Reportes.Catalogo;
using Application.Reportes.CrearReporte;
using Application.Reportes.EliminarReporte;
using Application.Reportes.ObtenerReportePorId;
using Application.Reportes.ObtenerReportes;
using Application.Reportes.RechazarReporte;
using Domain.Reportes;
using Microsoft.AspNetCore.Mvc;
using API.DTOs.Reportes;
using Application.Reportes;

namespace API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ICrearReporteHandler _crearReporteHandler;
    private readonly IObtenerReportesHandler _obtenerReportesHandler;
    private readonly IObtenerReportePorIdHandler _obtenerReportePorIdHandler;
    private readonly IActualizarEstadoReporteHandler _actualizarEstadoReporteHandler;
    private readonly IActualizarNivelEmergenciaReporteHandler _actualizarNivelEmergenciaReporteHandler;
    private readonly IAsignarResponsableReporteHandler _asignarResponsableReporteHandler;
    private readonly IRechazarReporteHandler _rechazarReporteHandler;
    private readonly IEliminarReporteHandler _eliminarReporteHandler;

    public ReportesController(
        ICrearReporteHandler crearReporteHandler,
        IObtenerReportesHandler obtenerReportesHandler,
        IObtenerReportePorIdHandler obtenerReportePorIdHandler,
        IActualizarEstadoReporteHandler actualizarEstadoReporteHandler,
        IActualizarNivelEmergenciaReporteHandler actualizarNivelEmergenciaReporteHandler,
        IAsignarResponsableReporteHandler asignarResponsableReporteHandler,
        IRechazarReporteHandler rechazarReporteHandler,
        IEliminarReporteHandler eliminarReporteHandler)
    {
        _crearReporteHandler = crearReporteHandler;
        _obtenerReportesHandler = obtenerReportesHandler;
        _obtenerReportePorIdHandler = obtenerReportePorIdHandler;
        _actualizarEstadoReporteHandler = actualizarEstadoReporteHandler;
        _actualizarNivelEmergenciaReporteHandler = actualizarNivelEmergenciaReporteHandler;
        _asignarResponsableReporteHandler = asignarResponsableReporteHandler;
        _rechazarReporteHandler = rechazarReporteHandler;
        _eliminarReporteHandler = eliminarReporteHandler;
    }

    [HttpPost]
    [EndpointSummary("Crear un reporte")]
    [EndpointDescription("""
        Crea un nuevo reporte de daño urbano.

        El cliente debe proporcionar la categoría y el tipo de reporte
        (códigos de GET /api/v1/Reportes/catalogo; el tipo debe
        pertenecer a la categoría), descripción, identificador de
        la coordenada y URL de la imagen.

        La fecha de creación, el identificador del reporte, el
        nivel de emergencia inicial y el usuario (genérico mientras
        no exista autenticación) son establecidos internamente
        por el servicio.
        """)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearReporte(
        [FromBody] CrearReporteRequest request,
        CancellationToken cancellationToken)
    {
        CrearReporteCommand command = new CrearReporteCommand(
            request.Categoria,
            request.Tipo,
            request.Descripcion,
            request.IdCoordenada,
            request.UrlImagen);

        Guid idReporte = await _crearReporteHandler.Handle(command, cancellationToken);

        return Created(
            $"/api/v1/Reportes/{idReporte}",
            new
            {
                IdReporte = idReporte,
                message = "Reporte creado"
            });
    }

    [HttpGet("catalogo")]
    [EndpointSummary("Obtener el catálogo de reportes")]
    [EndpointDescription("""
        Obtiene las categorías de reporte con sus tipos.

        Los códigos son los valores que se envían en "categoria" y
        "tipo" al crear un reporte; los nombres son los textos
        para mostrar al usuario.
        """)]
    [ProducesResponseType<IReadOnlyList<CategoriaReporteDto>>(StatusCodes.Status200OK)]
    public IActionResult ObtenerCatalogo() => Ok(CategoriaReporteDto.DesdeCatalogo());

    [HttpGet]
    [EndpointSummary("Obtener reportes")]
    [EndpointDescription("""
        Obtiene una página de reportes registrados en el sistema.

        Permite filtrar opcionalmente por estado y nivel de emergencia.
        El tamaño de página está limitado a un máximo de 100 elementos.
        """)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerReportes(
        [FromQuery] EstadoReporte? estado,
        [FromQuery] NivelEmergencia? nivelEmergencia,
        [FromQuery] int pagina,
        [FromQuery] int tamanoPagina,
        CancellationToken cancellationToken)
    {
        int paginaNormalizada = pagina < 1 ? 1 : pagina;
        int tamanoPaginaNormalizado = tamanoPagina switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => tamanoPagina
        };

        PaginaDto<ReporteDto> reportes = await _obtenerReportesHandler.Handle(
            new ObtenerReportesQuery(estado, nivelEmergencia, paginaNormalizada, tamanoPaginaNormalizado),
            cancellationToken);

        return Ok(reportes);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Obtener un reporte por ID")]
    [EndpointDescription("""
        Obtiene la información detallada de un reporte utilizando
        su identificador único.

        El identificador debe ser un UUID válido.
        """)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerReporte(
        Guid id,
        CancellationToken cancellationToken)
    {
        ReporteDto? reporte = await _obtenerReportePorIdHandler.Handle(new ObtenerReportePorIdQuery(id), cancellationToken);

        return reporte is null ? NotFound() : Ok(reporte);
    }

    [HttpPatch("{id:guid}/estado")]
    [EndpointSummary("Actualizar el estado de un reporte")]
    [EndpointDescription("""
        Actualiza el estado actual de un reporte.

        El reporte debe existir y el nuevo estado debe ser válido
        según las reglas del dominio.
        """)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarEstado(
        Guid id,
        [FromBody] ActualizarEstadoRequest request,
        CancellationToken cancellationToken)
    {
        bool actualizado = await _actualizarEstadoReporteHandler.Handle(
            new ActualizarEstadoReporteCommand(id, request.Estado), cancellationToken);

        return actualizado ? Ok() : NotFound();
    }

    [HttpPatch("{id:guid}/nivel-emergencia")]
    [EndpointSummary("Actualizar el nivel de emergencia")]
    [EndpointDescription("""
        Actualiza el nivel de emergencia asignado a un reporte.

        El reporte debe existir y el nivel de emergencia debe ser válido.
        """)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarNivelEmergencia(
        Guid id,
        [FromBody] ActualizarNivelEmergenciaRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.NivelEmergencia))
        {
            return BadRequest(new { message = "El nivel de emergencia indicado no es válido." });
        }

        bool actualizado = await _actualizarNivelEmergenciaReporteHandler.Handle(
            new ActualizarNivelEmergenciaReporteCommand(id, request.NivelEmergencia), cancellationToken);

        return actualizado ? Ok() : NotFound();
    }

    [HttpPatch("{id:guid}/asignacion")]
    [EndpointSummary("Asignar responsable a un reporte")]
    [EndpointDescription("""
        Asigna un usuario responsable a un reporte.

        El responsable debe corresponder a un usuario válido
        dentro del sistema.
        """)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AsignarResponsable(
        Guid id,
        [FromBody] AsignarResponsableRequest request,
        CancellationToken cancellationToken)
    {
        bool asignado = await _asignarResponsableReporteHandler.Handle(
            new AsignarResponsableReporteCommand(id, request.IdResponsable), cancellationToken);

        return asignado ? Ok() : NotFound();
    }

    [HttpPut("{id:guid}/rechazo")]
    [EndpointSummary("Rechazar un reporte")]
    [EndpointDescription("""
        Rechaza un reporte y registra el motivo del rechazo.

        El motivo del rechazo es obligatorio.
        """)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RechazarReporte(
        Guid id,
        [FromBody] RechazarReporteRequest request,
        CancellationToken cancellationToken)
    {
        bool rechazado = await _rechazarReporteHandler.Handle(
            new RechazarReporteCommand(id, request.Motivo), cancellationToken);

        return rechazado ? Ok() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Eliminar un reporte")]
    [EndpointDescription("""
        Elimina un reporte utilizando su identificador único.

        El reporte debe existir para poder ser eliminado.
        """)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarReporte(Guid id, CancellationToken cancellationToken)
    {
        bool eliminado = await _eliminarReporteHandler.Handle(new EliminarReporteCommand(id), cancellationToken);

        return eliminado ? NoContent() : NotFound();
    }
}

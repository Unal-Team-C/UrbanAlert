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
using Application.Reportes.CrearReporte;
using Application.Reportes.EliminarReporte;
using Application.Reportes.ObtenerReportePorId;
using Application.Reportes.RechazarReporte;
using Domain.Reportes;
using Microsoft.AspNetCore.Authorization;
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

        El cliente debe proporcionar el tipo de daño, descripción,
        identificador de la coordenada, URL de la imagen e
        identificador del usuario.

        La fecha de creación, el identificador del reporte y el
        nivel de emergencia inicial son establecidos internamente
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
            request.TipoDano,
            request.Descripcion,
            request.IdCoordenada,
            request.UrlImagen,
            request.IdUsuario);

        Guid idReporte = await _crearReporteHandler.Handle(command, cancellationToken);

        return Created(
            $"/api/v1/Reportes/{idReporte}",
            new
            {
                IdReporte = idReporte,
                message = "Reporte creado"
            });
    }

    [HttpGet]
    [EndpointSummary("Obtener todos los reportes")]
    [EndpointDescription("""
        Obtiene la colección de reportes registrados en el sistema.

        Cada reporte contiene información sobre el tipo de daño,
        descripción, fecha, coordenadas, imagen, usuario y estado.
        """)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerReportes(CancellationToken cancellationToken)
    {
        IReadOnlyList<ReporteDto> reportes = await _obtenerReportesHandler.Handle(cancellationToken);

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
        if (!Enum.TryParse<EstadoReporte>(request.Estado, ignoreCase: true, out EstadoReporte nuevoEstado))
        {
            return BadRequest(new { message = "El estado indicado no es válido." });
        }

        bool actualizado = await _actualizarEstadoReporteHandler.Handle(
            new ActualizarEstadoReporteCommand(id, nuevoEstado), cancellationToken);

        return actualizado ? Ok() : NotFound();
    }

    [HttpPatch("{id:guid}/nivel-emergencia")]
    [Authorize(Roles = "Administrador")]
    [EndpointSummary("Actualizar el nivel de emergencia")]
    [EndpointDescription("""
        Actualiza el nivel de emergencia asignado a un reporte.

        Esta operación requiere permisos de Administrador.
        El reporte debe existir y el nivel de emergencia debe ser válido.
        """)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarNivelEmergencia(
        Guid id,
        [FromBody] ActualizarNivelEmergenciaRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(NivelEmergencia), request.NivelEmergencia))
        {
            return BadRequest(new { message = "El nivel de emergencia indicado no es válido." });
        }

        bool actualizado = await _actualizarNivelEmergenciaReporteHandler.Handle(
            new ActualizarNivelEmergenciaReporteCommand(id, (NivelEmergencia)request.NivelEmergencia), cancellationToken);

        return actualizado ? Ok() : NotFound();
    }

    [HttpPatch("{id:guid}/asignacion")]
    [Authorize(Roles = "Administrador")]
    [EndpointSummary("Asignar responsable a un reporte")]
    [EndpointDescription("""
        Asigna un usuario responsable a un reporte.

        Esta operación requiere permisos de Administrador.
        El responsable debe corresponder a un usuario válido
        dentro del sistema.
        """)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    [Authorize(Roles = "Administrador")]
    [EndpointSummary("Rechazar un reporte")]
    [EndpointDescription("""
        Rechaza un reporte y registra el motivo del rechazo.

        Esta operación requiere permisos de Administrador.
        El motivo del rechazo es obligatorio.
        """)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    [Authorize(Roles = "Administrador")]
    [EndpointSummary("Eliminar un reporte")]
    [EndpointDescription("""
        Elimina un reporte utilizando su identificador único.

        Esta operación requiere permisos de Administrador.
        El reporte debe existir para poder ser eliminado.
        """)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarReporte(Guid id, CancellationToken cancellationToken)
    {
        bool eliminado = await _eliminarReporteHandler.Handle(new EliminarReporteCommand(id), cancellationToken);

        return eliminado ? NoContent() : NotFound();
    }
}

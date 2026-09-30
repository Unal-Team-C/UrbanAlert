using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DTOs.Reportes;

namespace API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReportesController : ControllerBase
{
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
    public IActionResult CrearReporte(
        [FromBody] CrearReporteRequest request)
    {
        var idReporte = Guid.NewGuid();

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
    public IActionResult ObtenerReportes()
    {
        return Ok();
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
    public IActionResult ObtenerReporte(
        Guid id)
    {
        return Ok();
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
    public IActionResult ActualizarEstado(
        Guid id,
        [FromBody] ActualizarEstadoRequest request)
    {
        return Ok();
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
    public IActionResult ActualizarNivelEmergencia(
        Guid id,
        [FromBody] ActualizarNivelEmergenciaRequest request)
    {
        return Ok();
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
    public IActionResult AsignarResponsable(
        Guid id,
        [FromBody] AsignarResponsableRequest request)
    {
        return Ok();
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
    public IActionResult RechazarReporte(
        Guid id,
        [FromBody] RechazarReporteRequest request)
    {
        return Ok();
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
    public IActionResult EliminarReporte(Guid id)
    {
        return NoContent();
    }
}

using API.DTOs.Reportes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReportesController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult CrearReporte([FromBody] CrearReporteRequest request)
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ObtenerReportes()
    {
        return Ok("Hello World!");
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ObtenerReporte(Guid id)
    {
        return Ok();
    }
    
    [HttpPatch("{id:guid}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ActualizarEstado(Guid id,
        [FromBody] ActualizarEstadoRequest request)
    {
        return Ok();
    }
    
    [HttpPatch("{id:guid}/nivel-emergencia")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AsignarResponsable(Guid id,
        [FromBody] AsignarResponsableRequest request)
    {
        return Ok();
    }
    
    [HttpPut("{id:guid}/rechazo")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RechazarReporte(Guid id,
        [FromBody] RechazarReporteRequest request)
    {
        return Ok();
    }
}
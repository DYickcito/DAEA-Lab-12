using Microsoft.AspNetCore.Mvc;
using UmaSalud.Entidades.Dtos;
using UmaSalud.Negocio;
using UmaSalud.Negocio.Servicios;

namespace DAEA_Lab_12.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacientesController(IPacienteServicio servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PacienteDto>>> Get([FromQuery] string? nombre, [FromQuery] int? especialidadId) => Ok(await servicio.ListarAsync(nombre, especialidadId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PacienteDto>> Get(int id)
    {
        var item = await servicio.ObtenerAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<PacienteDto>> Post(PacienteCrearDto dto)
    {
        try { var item = await servicio.CrearAsync(dto); return CreatedAtAction(nameof(Get), new { id = item.Id }, item); }
        catch (ReglaNegocioException ex) { return Conflict(ex.Message); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, PacienteActualizarDto dto)
    {
        try { return await servicio.ActualizarAsync(id, dto) ? NoContent() : NotFound(); }
        catch (ReglaNegocioException ex) { return Conflict(ex.Message); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => await servicio.EliminarAsync(id) ? NoContent() : NotFound();
}

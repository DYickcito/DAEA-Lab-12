using Microsoft.AspNetCore.Mvc;
using UmaSalud.Entidades.Dtos;
using UmaSalud.Negocio;
using UmaSalud.Negocio.Servicios;

namespace DAEA_Lab_12.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EspecialidadesController(IEspecialidadServicio servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<EspecialidadDto>>> Get() => Ok(await servicio.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EspecialidadDto>> Get(int id)
    {
        var item = await servicio.ObtenerAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("{id:int}/pacientes")]
    public async Task<ActionResult<List<PacienteDto>>> Pacientes(int id)
    {
        var items = await servicio.ListarPacientesAsync(id);
        return items is null ? NotFound() : Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<EspecialidadDto>> Post(EspecialidadCrearDto dto)
    {
        try { var item = await servicio.CrearAsync(dto); return CreatedAtAction(nameof(Get), new { id = item.Id }, item); }
        catch (ReglaNegocioException ex) { return Conflict(ex.Message); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, EspecialidadActualizarDto dto)
    {
        try { return await servicio.ActualizarAsync(id, dto) ? NoContent() : NotFound(); }
        catch (ReglaNegocioException ex) { return Conflict(ex.Message); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { return await servicio.EliminarAsync(id) ? NoContent() : NotFound(); }
        catch (ReglaNegocioException ex) { return Conflict(ex.Message); }
    }
}

using DAEA_Lab_12.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace DAEA_Lab_12.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EspecialidadesController(ClinicaContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Especialidad>>> Get() => Ok(await db.Especialidades.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Especialidad>> Get(int id)
    {
        var item = await db.Especialidades.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("{id:int}/pacientes")]
    public async Task<ActionResult<IEnumerable<Paciente>>> Pacientes(int id)
    {
        if (!await db.Especialidades.AnyAsync(e => e.Id == id)) return NotFound();
        return Ok(await db.Pacientes.AsNoTracking().Where(p => p.EspecialidadId == id).OrderBy(p => p.Nombre).ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<Especialidad>> Post(Especialidad item)
    {
        if (await db.Especialidades.AnyAsync(e => e.Nombre == item.Nombre)) return Conflict("Ya existe una especialidad con ese nombre.");
        item.Id = 0;
        item.Pacientes.Clear();
        db.Especialidades.Add(item);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 }) { return Conflict("Ya existe una especialidad con ese nombre."); }
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Especialidad item)
    {
        if (id != item.Id) return BadRequest("El ID de la ruta no coincide con el cuerpo.");
        var actual = await db.Especialidades.FindAsync(id);
        if (actual is null) return NotFound();
        if (await db.Especialidades.AnyAsync(e => e.Nombre == item.Nombre && e.Id != id)) return Conflict("Ya existe una especialidad con ese nombre.");
        actual.Nombre = item.Nombre; actual.Descripcion = item.Descripcion;
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 }) { return Conflict("Ya existe una especialidad con ese nombre."); }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Especialidades.FindAsync(id);
        if (item is null) return NotFound();
        if (await db.Pacientes.AnyAsync(p => p.EspecialidadId == id)) return Conflict("No se puede eliminar una especialidad con pacientes.");
        db.Especialidades.Remove(item);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 }) { return Conflict("No se puede eliminar una especialidad con pacientes."); }
        return NoContent();
    }
}


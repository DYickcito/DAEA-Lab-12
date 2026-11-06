using DAEA_Lab_12.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace DAEA_Lab_12.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacientesController(ClinicaContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Paciente>>> Get([FromQuery] string? nombre, [FromQuery] int? especialidadId)
    {
        var query = db.Pacientes.AsNoTracking().Include(p => p.Especialidad).AsQueryable();
        if (!string.IsNullOrWhiteSpace(nombre)) query = query.Where(p => p.Nombre.Contains(nombre));
        if (especialidadId.HasValue) query = query.Where(p => p.EspecialidadId == especialidadId);
        return Ok(await query.OrderBy(p => p.Nombre).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Paciente>> Get(int id)
    {
        var item = await db.Pacientes.AsNoTracking().Include(p => p.Especialidad).FirstOrDefaultAsync(p => p.Id == id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Paciente>> Post(Paciente item)
    {
        if (!await db.Especialidades.AnyAsync(e => e.Id == item.EspecialidadId)) return BadRequest("La especialidad no existe.");
        if (await db.Pacientes.AnyAsync(p => p.HistoriaClinica == item.HistoriaClinica)) return Conflict("La historia clínica ya existe.");
        item.Id = 0; item.Especialidad = null;
        db.Pacientes.Add(item);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 }) { return Conflict("La historia clínica ya existe."); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 }) { return BadRequest("La especialidad no existe."); }
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Paciente item)
    {
        if (id != item.Id) return BadRequest("El ID de la ruta no coincide con el cuerpo.");
        var actual = await db.Pacientes.FindAsync(id);
        if (actual is null) return NotFound();
        if (!await db.Especialidades.AnyAsync(e => e.Id == item.EspecialidadId)) return BadRequest("La especialidad no existe.");
        if (await db.Pacientes.AnyAsync(p => p.HistoriaClinica == item.HistoriaClinica && p.Id != id)) return Conflict("La historia clínica ya existe.");
        actual.Nombre = item.Nombre; actual.HistoriaClinica = item.HistoriaClinica; actual.Edad = item.Edad;
        actual.MotivoConsulta = item.MotivoConsulta; actual.EspecialidadId = item.EspecialidadId;
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 }) { return Conflict("La historia clínica ya existe."); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 }) { return BadRequest("La especialidad no existe."); }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Pacientes.FindAsync(id);
        if (item is null) return NotFound();
        db.Pacientes.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}


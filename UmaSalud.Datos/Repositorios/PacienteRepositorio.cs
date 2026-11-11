using Microsoft.EntityFrameworkCore;
using UmaSalud.Entidades.Models;

namespace UmaSalud.Datos.Repositorios;

public class PacienteRepositorio(ClinicaContext db) : IPacienteRepositorio
{
    public Task<List<Paciente>> ListarAsync(string? nombre = null, int? especialidadId = null)
    {
        var consulta = db.Pacientes.AsNoTracking().Include(p => p.Especialidad).AsQueryable();
        if (!string.IsNullOrWhiteSpace(nombre)) consulta = consulta.Where(p => p.Nombre.Contains(nombre));
        if (especialidadId.HasValue) consulta = consulta.Where(p => p.EspecialidadId == especialidadId);
        return consulta.OrderBy(p => p.Nombre).ToListAsync();
    }
    public Task<Paciente?> ObtenerAsync(int id) => db.Pacientes.Include(p => p.Especialidad).FirstOrDefaultAsync(p => p.Id == id);
    public Task<bool> ExisteHistoriaAsync(string historia, int? exceptoId = null) => db.Pacientes.AnyAsync(p => p.HistoriaClinica == historia && p.Id != exceptoId);
    public async Task AgregarAsync(Paciente item) => await db.Pacientes.AddAsync(item);
    public Task EliminarAsync(Paciente item) { db.Pacientes.Remove(item); return Task.CompletedTask; }
    public async Task GuardarAsync() => await db.SaveChangesAsync();
}

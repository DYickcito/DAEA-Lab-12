using Microsoft.EntityFrameworkCore;
using UmaSalud.Entidades.Models;

namespace UmaSalud.Datos.Repositorios;

public class EspecialidadRepositorio(ClinicaContext db) : IEspecialidadRepositorio
{
    public Task<List<Especialidad>> ListarAsync() => db.Especialidades.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync();
    public Task<Especialidad?> ObtenerAsync(int id) => db.Especialidades.FirstOrDefaultAsync(e => e.Id == id);
    public Task<bool> ExisteNombreAsync(string nombre, int? exceptoId = null) => db.Especialidades.AnyAsync(e => e.Nombre == nombre && e.Id != exceptoId);
    public Task<bool> TienePacientesAsync(int id) => db.Pacientes.AnyAsync(p => p.EspecialidadId == id);
    public async Task AgregarAsync(Especialidad item) => await db.Especialidades.AddAsync(item);
    public Task EliminarAsync(Especialidad item) { db.Especialidades.Remove(item); return Task.CompletedTask; }
    public async Task GuardarAsync() => await db.SaveChangesAsync();
}

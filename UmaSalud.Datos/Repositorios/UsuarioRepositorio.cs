using Microsoft.EntityFrameworkCore;
using UmaSalud.Entidades.Models;

namespace UmaSalud.Datos.Repositorios;

public class UsuarioRepositorio(ClinicaContext db) : IUsuarioRepositorio
{
    public Task<Usuario?> ObtenerPorCorreoAsync(string correo) =>
        db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);

    public async Task AgregarAsync(Usuario usuario) => await db.Usuarios.AddAsync(usuario);

    public async Task GuardarAsync() => await db.SaveChangesAsync();
}

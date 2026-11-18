using UmaSalud.Entidades.Models;

namespace UmaSalud.Datos.Repositorios;

// Operaciones de datos sobre la tabla usuarios (mismo patrón que los demás repositorios).
public interface IUsuarioRepositorio
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);
    Task AgregarAsync(Usuario usuario);
    Task GuardarAsync();
}

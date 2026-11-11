using UmaSalud.Entidades.Models;

namespace UmaSalud.Datos.Repositorios;

public interface IEspecialidadRepositorio
{
    Task<List<Especialidad>> ListarAsync();
    Task<Especialidad?> ObtenerAsync(int id);
    Task<bool> ExisteNombreAsync(string nombre, int? exceptoId = null);
    Task<bool> TienePacientesAsync(int id);
    Task AgregarAsync(Especialidad item);
    Task EliminarAsync(Especialidad item);
    Task GuardarAsync();
}

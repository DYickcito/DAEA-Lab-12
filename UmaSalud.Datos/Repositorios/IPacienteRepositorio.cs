using UmaSalud.Entidades.Models;

namespace UmaSalud.Datos.Repositorios;

public interface IPacienteRepositorio
{
    Task<List<Paciente>> ListarAsync(string? nombre = null, int? especialidadId = null);
    Task<Paciente?> ObtenerAsync(int id);
    Task<bool> ExisteHistoriaAsync(string historia, int? exceptoId = null);
    Task AgregarAsync(Paciente item);
    Task EliminarAsync(Paciente item);
    Task GuardarAsync();
}

using UmaSalud.Entidades.Dtos;

namespace UmaSalud.Negocio.Servicios;

public interface IPacienteServicio
{
    Task<List<PacienteDto>> ListarAsync(string? nombre, int? especialidadId);
    Task<PacienteDto?> ObtenerAsync(int id);
    Task<PacienteDto> CrearAsync(PacienteCrearDto dto);
    Task<bool> ActualizarAsync(int id, PacienteActualizarDto dto);
    Task<bool> EliminarAsync(int id);
}

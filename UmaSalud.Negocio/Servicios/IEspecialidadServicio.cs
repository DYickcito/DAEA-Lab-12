using UmaSalud.Entidades.Dtos;

namespace UmaSalud.Negocio.Servicios;

public interface IEspecialidadServicio
{
    Task<List<EspecialidadDto>> ListarAsync();
    Task<EspecialidadDto?> ObtenerAsync(int id);
    Task<List<PacienteDto>?> ListarPacientesAsync(int id);
    Task<EspecialidadDto> CrearAsync(EspecialidadCrearDto dto);
    Task<bool> ActualizarAsync(int id, EspecialidadActualizarDto dto);
    Task<bool> EliminarAsync(int id);
}

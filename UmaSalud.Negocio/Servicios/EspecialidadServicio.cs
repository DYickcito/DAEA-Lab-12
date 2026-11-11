using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UmaSalud.Datos.Repositorios;
using UmaSalud.Entidades.Dtos;
using UmaSalud.Entidades.Models;

namespace UmaSalud.Negocio.Servicios;

public class EspecialidadServicio(IEspecialidadRepositorio repositorio, IPacienteRepositorio pacientes, IMapper mapper) : IEspecialidadServicio
{
    public async Task<List<EspecialidadDto>> ListarAsync() => mapper.Map<List<EspecialidadDto>>(await repositorio.ListarAsync());
    public async Task<EspecialidadDto?> ObtenerAsync(int id) => mapper.Map<EspecialidadDto?>(await repositorio.ObtenerAsync(id));
    public async Task<List<PacienteDto>?> ListarPacientesAsync(int id)
    {
        if (await repositorio.ObtenerAsync(id) is null) return null;
        return mapper.Map<List<PacienteDto>>(await pacientes.ListarAsync(especialidadId: id));
    }
    public async Task<EspecialidadDto> CrearAsync(EspecialidadCrearDto dto)
    {
        if (await repositorio.ExisteNombreAsync(dto.Nombre)) throw new ReglaNegocioException("Ya existe una especialidad con ese nombre.");
        var item = mapper.Map<Especialidad>(dto);
        await repositorio.AgregarAsync(item);
        try { await repositorio.GuardarAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 }) { throw new ReglaNegocioException("Ya existe una especialidad con ese nombre."); }
        return mapper.Map<EspecialidadDto>(item);
    }
    public async Task<bool> ActualizarAsync(int id, EspecialidadActualizarDto dto)
    {
        var item = await repositorio.ObtenerAsync(id);
        if (item is null) return false;
        if (await repositorio.ExisteNombreAsync(dto.Nombre, id)) throw new ReglaNegocioException("Ya existe una especialidad con ese nombre.");
        mapper.Map(dto, item);
        try { await repositorio.GuardarAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 }) { throw new ReglaNegocioException("Ya existe una especialidad con ese nombre."); }
        return true;
    }
    public async Task<bool> EliminarAsync(int id)
    {
        var item = await repositorio.ObtenerAsync(id);
        if (item is null) return false;
        if (await repositorio.TienePacientesAsync(id)) throw new ReglaNegocioException("No se puede eliminar una especialidad con pacientes.");
        await repositorio.EliminarAsync(item);
        try { await repositorio.GuardarAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 }) { throw new ReglaNegocioException("No se puede eliminar una especialidad con pacientes."); }
        return true;
    }
}

using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UmaSalud.Datos.Repositorios;
using UmaSalud.Entidades.Dtos;
using UmaSalud.Entidades.Models;

namespace UmaSalud.Negocio.Servicios;

public class PacienteServicio(IPacienteRepositorio repositorio, IEspecialidadRepositorio especialidades, IMapper mapper) : IPacienteServicio
{
    public async Task<List<PacienteDto>> ListarAsync(string? nombre, int? especialidadId) => mapper.Map<List<PacienteDto>>(await repositorio.ListarAsync(nombre, especialidadId));
    public async Task<PacienteDto?> ObtenerAsync(int id) => mapper.Map<PacienteDto?>(await repositorio.ObtenerAsync(id));
    public async Task<PacienteDto> CrearAsync(PacienteCrearDto dto)
    {
        var especialidad = await especialidades.ObtenerAsync(dto.EspecialidadId) ?? throw new ArgumentException("La especialidad no existe.");
        if (await repositorio.ExisteHistoriaAsync(dto.HistoriaClinica)) throw new ReglaNegocioException("La historia clínica ya existe.");
        var item = mapper.Map<Paciente>(dto);
        item.Especialidad = especialidad;
        await repositorio.AgregarAsync(item);
        await GuardarAsync();
        return mapper.Map<PacienteDto>(item);
    }
    public async Task<bool> ActualizarAsync(int id, PacienteActualizarDto dto)
    {
        var item = await repositorio.ObtenerAsync(id);
        if (item is null) return false;
        var especialidad = await especialidades.ObtenerAsync(dto.EspecialidadId) ?? throw new ArgumentException("La especialidad no existe.");
        if (await repositorio.ExisteHistoriaAsync(dto.HistoriaClinica, id)) throw new ReglaNegocioException("La historia clínica ya existe.");
        mapper.Map(dto, item);
        item.Especialidad = especialidad;
        await GuardarAsync();
        return true;
    }
    public async Task<bool> EliminarAsync(int id)
    {
        var item = await repositorio.ObtenerAsync(id);
        if (item is null) return false;
        await repositorio.EliminarAsync(item);
        await repositorio.GuardarAsync();
        return true;
    }
    private async Task GuardarAsync()
    {
        try { await repositorio.GuardarAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 }) { throw new ReglaNegocioException("La historia clínica ya existe."); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 }) { throw new ArgumentException("La especialidad no existe."); }
    }
}

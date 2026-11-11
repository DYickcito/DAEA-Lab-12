using AutoMapper;
using UmaSalud.Entidades.Dtos;
using UmaSalud.Entidades.Models;

namespace UmaSalud.Negocio;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Especialidad, EspecialidadDto>();
        CreateMap<EspecialidadCrearDto, Especialidad>()
            .ForMember(d => d.Id, o => o.Ignore()).ForMember(d => d.Pacientes, o => o.Ignore());
        CreateMap<EspecialidadActualizarDto, Especialidad>()
            .ForMember(d => d.Id, o => o.Ignore()).ForMember(d => d.Pacientes, o => o.Ignore());
        CreateMap<Paciente, PacienteDto>()
            .ForMember(d => d.EspecialidadNombre, o => o.MapFrom(s => s.Especialidad!.Nombre));
        CreateMap<PacienteCrearDto, Paciente>()
            .ForMember(d => d.Id, o => o.Ignore()).ForMember(d => d.Especialidad, o => o.Ignore());
        CreateMap<PacienteActualizarDto, Paciente>()
            .ForMember(d => d.Id, o => o.Ignore()).ForMember(d => d.Especialidad, o => o.Ignore());
    }
}

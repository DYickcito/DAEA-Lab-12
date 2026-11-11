using System.ComponentModel.DataAnnotations;

namespace UmaSalud.Entidades.Dtos;

public class PacienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string HistoriaClinica { get; set; } = null!;
    public int Edad { get; set; }
    public string MotivoConsulta { get; set; } = null!;
    public int EspecialidadId { get; set; }
    public string EspecialidadNombre { get; set; } = null!;
}

public class PacienteCrearDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = null!;
    [Required, StringLength(20, MinimumLength = 3)]
    public string HistoriaClinica { get; set; } = null!;
    [Range(1, 120)]
    public int Edad { get; set; }
    [Required, StringLength(200, MinimumLength = 3)]
    public string MotivoConsulta { get; set; } = null!;
    [Range(1, int.MaxValue)]
    public int EspecialidadId { get; set; }
}

public class PacienteActualizarDto : PacienteCrearDto;

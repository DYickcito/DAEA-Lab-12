using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DAEA_Lab_12.Models;

[Table("pacientes")]
[Index("HistoriaClinica", Name = "UQ_pacientes_historia", IsUnique = true)]
public partial class Paciente
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = null!;

    [Column("historia_clinica")]
    [Required, StringLength(20, MinimumLength = 3)]
    public string HistoriaClinica { get; set; } = null!;

    [Range(1, 120)]
    [Column("edad")]
    public int Edad { get; set; }

    [Column("motivo_consulta")]
    [Required, StringLength(200, MinimumLength = 3)]
    public string MotivoConsulta { get; set; } = null!;

    [Range(1, int.MaxValue)]
    [Column("especialidad_id")]
    public int EspecialidadId { get; set; }

    [ForeignKey("EspecialidadId")]
    [InverseProperty("Pacientes")]
    public virtual Especialidad? Especialidad { get; set; }
}


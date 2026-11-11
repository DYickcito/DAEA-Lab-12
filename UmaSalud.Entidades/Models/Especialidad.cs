using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UmaSalud.Entidades.Models;

[Table("especialidades")]
public partial class Especialidad
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [Required, StringLength(80, MinimumLength = 2)]
    public string Nombre { get; set; } = null!;

    [Column("descripcion")]
    [Required, StringLength(250, MinimumLength = 5)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("Especialidad")]
    public virtual ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
}

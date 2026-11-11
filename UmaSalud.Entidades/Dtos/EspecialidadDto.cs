using System.ComponentModel.DataAnnotations;

namespace UmaSalud.Entidades.Dtos;

public class EspecialidadDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
}

public class EspecialidadCrearDto
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string Nombre { get; set; } = null!;
    [Required, StringLength(250, MinimumLength = 5)]
    public string Descripcion { get; set; } = null!;
}

public class EspecialidadActualizarDto
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string Nombre { get; set; } = null!;
    [Required, StringLength(250, MinimumLength = 5)]
    public string Descripcion { get; set; } = null!;
}

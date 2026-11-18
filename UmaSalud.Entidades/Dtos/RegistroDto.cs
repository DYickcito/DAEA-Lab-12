using System.ComponentModel.DataAnnotations;

namespace UmaSalud.Entidades.Dtos;

// DTO de entrada para registrarse. No incluye Rol: lo decide el servidor,
// nunca el cliente (si no, cualquiera podría registrarse como Medico).
public class RegistroDto
{
    [Required, EmailAddress, StringLength(100)]
    public string Correo { get; set; } = null!;

    [Required, StringLength(50, MinimumLength = 8)]
    public string Clave { get; set; } = null!;
}

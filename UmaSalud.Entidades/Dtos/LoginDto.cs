using System.ComponentModel.DataAnnotations;

namespace UmaSalud.Entidades.Dtos;

// DTO de entrada para iniciar sesión.
public class LoginDto
{
    [Required, EmailAddress, StringLength(100)]
    public string Correo { get; set; } = null!;

    // Sin mínimo: cualquier clave incorrecta debe llegar al servicio y responder 401,
    // no un 400 de validación.
    [Required, StringLength(50)]
    public string Clave { get; set; } = null!;
}

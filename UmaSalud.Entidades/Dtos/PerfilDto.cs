namespace UmaSalud.Entidades.Dtos;

// DTO de salida de GET api/auth/perfil. Se arma con los claims del token,
// no consultando la base de datos.
public class PerfilDto
{
    public string Correo { get; set; } = null!;
    public string Rol { get; set; } = null!;
}

namespace UmaSalud.Entidades.Dtos;

// DTO de salida del login: el token firmado y cuándo vence.
public class TokenDto
{
    public string Token { get; set; } = null!;
    public DateTime Expira { get; set; }
}

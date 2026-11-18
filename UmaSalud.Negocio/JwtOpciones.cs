namespace UmaSalud.Negocio;

// Valores de la sección "Jwt" de la configuración (user-secrets o variables de entorno).
// Se usan en dos lugares: para GENERAR el token (AuthServicio) y para VALIDARLO (Program.cs).
public class JwtOpciones
{
    public string Clave { get; set; } = null!;      // clave secreta de al menos 32 caracteres
    public string Emisor { get; set; } = null!;     // quién genera el token
    public string Audiencia { get; set; } = null!;  // a quién va dirigido
    public int Minutos { get; set; }                // vida del token
}

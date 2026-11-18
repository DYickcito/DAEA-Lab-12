using UmaSalud.Entidades.Dtos;

namespace UmaSalud.Negocio.Servicios;

// Contrato de seguridad de la capa Negocio. Recibe y devuelve DTOs.
public interface IAuthServicio
{
    Task RegistrarAsync(RegistroDto dto);
    Task<TokenDto?> LoginAsync(LoginDto dto);   // null = credenciales inválidas
}

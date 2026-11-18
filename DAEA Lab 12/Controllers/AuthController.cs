using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UmaSalud.Entidades.Dtos;
using UmaSalud.Negocio;
using UmaSalud.Negocio.Servicios;

namespace DAEA_Lab_12.Controllers;

// CAPA API: controlador delgado de seguridad.
// registro y login son públicos (todavía no hay token); perfil exige autenticación.
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthServicio auth) : ControllerBase
{
    // POST api/auth/registro -> 201, 400 (validación) o 409 (correo repetido)
    [AllowAnonymous]
    [HttpPost("registro")]
    public async Task<IActionResult> Registro(RegistroDto dto)
    {
        try
        {
            await auth.RegistrarAsync(dto);
            return StatusCode(StatusCodes.Status201Created);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new ProblemDetails { Title = "Regla de negocio", Detail = ex.Message });
        }
    }

    // POST api/auth/login -> 200 con el token, o 401 si las credenciales no son válidas
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login(LoginDto dto)
    {
        var token = await auth.LoginAsync(dto);
        return token is null ? Unauthorized() : Ok(token);
    }

    // GET api/auth/perfil -> 200 solo con token. Los datos salen de los claims,
    // no de la base de datos.
    [Authorize]
    [HttpGet("perfil")]
    public ActionResult<PerfilDto> Perfil()
    {
        var correo = User.FindFirstValue(ClaimTypes.Email)
                     ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
                     ?? string.Empty;
        var rol = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        return Ok(new PerfilDto { Correo = correo, Rol = rol });
    }
}

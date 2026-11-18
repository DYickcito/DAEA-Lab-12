using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UmaSalud.Datos.Repositorios;
using UmaSalud.Entidades;
using UmaSalud.Entidades.Dtos;
using UmaSalud.Entidades.Models;

namespace UmaSalud.Negocio.Servicios;

// CAPA NEGOCIO: reglas de seguridad. No conoce HTTP: devuelve null o lanza
// ReglaNegocioException y el controlador decide el código (401, 409...).
public class AuthServicio(IUsuarioRepositorio repositorio, JwtOpciones jwt) : IAuthServicio
{
    public async Task RegistrarAsync(RegistroDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();

        // Regla de negocio: el correo debe ser único.
        if (await repositorio.ObtenerPorCorreoAsync(correo) is not null)
            throw new ReglaNegocioException("Ya existe un usuario con ese correo.");

        var usuario = new Usuario
        {
            Correo = correo,
            ClaveHash = BCrypt.Net.BCrypt.HashPassword(dto.Clave),  // se guarda el hash, nunca la clave
            Rol = Roles.Recepcionista                               // el rol lo decide el servidor
        };

        await repositorio.AgregarAsync(usuario);
        try
        {
            await repositorio.GuardarAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ReglaNegocioException("Ya existe un usuario con ese correo.");
        }
    }

    public async Task<TokenDto?> LoginAsync(LoginDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();
        var usuario = await repositorio.ObtenerPorCorreoAsync(correo);

        // Mismo resultado si el correo no existe o si la clave es incorrecta:
        // el cliente no debe saber cuál de las dos falló.
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Clave, usuario.ClaveHash))
            return null;

        return GenerarToken(usuario);
    }

    private TokenDto GenerarToken(Usuario usuario)
    {
        // Claims mínimos: sub (id), email y rol.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.Rol)     // lo usan [Authorize(Roles = ...)]
        };

        // La clave secreta firma el token: si alguien lo altera, la firma deja de coincidir.
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave));
        var expira = DateTime.UtcNow.AddMinutes(jwt.Minutos);

        var token = new JwtSecurityToken(
            issuer: jwt.Emisor,
            audience: jwt.Audiencia,
            claims: claims,
            expires: expira,
            signingCredentials: new SigningCredentials(clave, SecurityAlgorithms.HmacSha256));

        return new TokenDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),  // encabezado.claims.firma
            Expira = expira
        };
    }
}

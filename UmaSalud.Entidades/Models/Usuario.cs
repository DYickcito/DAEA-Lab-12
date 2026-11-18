using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UmaSalud.Entidades.Models;

// Entidad de la tabla dbo.usuarios (seguridad JWT, S14).
[Table("usuarios")]
public partial class Usuario
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    // Correo único: es el nombre de usuario y también viaja en el claim "email".
    [Column("correo")]
    [Required, EmailAddress, StringLength(100)]
    public string Correo { get; set; } = null!;

    // Nunca se guarda la clave: solo el hash BCrypt.
    [Column("clave_hash")]
    [Required, StringLength(100)]
    public string ClaveHash { get; set; } = null!;

    // Rol del negocio: Recepcionista o Medico. Lo decide el servidor.
    [Column("rol")]
    [Required, StringLength(20)]
    public string Rol { get; set; } = null!;
}

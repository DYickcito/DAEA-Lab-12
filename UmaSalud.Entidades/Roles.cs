namespace UmaSalud.Entidades;

// Roles propios del negocio de Uma Salud.
// Se usan en [Authorize(Roles = ...)] y al asignar el rol en el registro.
public static class Roles
{
    public const string Recepcionista = "Recepcionista";
    public const string Medico = "Medico";
}

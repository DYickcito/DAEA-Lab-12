using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// MICROSERVICIO 2: CitasApi.
// Proyecto independiente de la API Uma Salud: no comparte base de datos ni clases.
// Los datos viven en memoria para mantener la demo corta, tal como pide el laboratorio.
// Valida el token JWT con la MISMA clave, emisor y audiencia que la API y el gateway.

var jwt = builder.Configuration.GetSection("Jwt");
var clave = jwt["Clave"];
var emisor = jwt["Emisor"];
var audiencia = jwt["Audiencia"];

if (string.IsNullOrWhiteSpace(clave) || clave.Length < 32)
    throw new InvalidOperationException(
        "Falta configurar Jwt:Clave (mínimo 32 caracteres). Use 'dotnet user-secrets set \"Jwt:Clave\" \"...\"' " +
        "o una variable de entorno como Jwt__Clave.");
if (string.IsNullOrWhiteSpace(emisor) || string.IsNullOrWhiteSpace(audiencia))
    throw new InvalidOperationException(
        "Configure Jwt:Emisor y Jwt:Audiencia (user-secrets o variables de entorno).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = emisor,
            ValidateAudience = true, ValidAudience = audiencia,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(clave))
        });
builder.Services.AddAuthorization();

// Datos de ejemplo en memoria, acordes al tema de la clínica Uma Salud.
var citas = new List<Cita>
{
    new(1, "Special Week", "UMA-001", "Medicina general", "2026-11-26T09:00", "Programada", "Dra. Alicia Vega"),
    new(2, "Silence Suzuka", "UMA-002", "Medicina deportiva", "2026-11-26T10:30", "Programada", "Dr. Bruno Salas"),
    new(3, "Tokai Teio", "UMA-003", "Medicina deportiva", "2026-11-27T08:15", "Confirmada", "Dr. Bruno Salas"),
    new(4, "Oguri Cap", "UMA-004", "Nutrición", "2026-11-27T11:00", "Atendida", "Dra. Carla Ríos"),
    new(5, "Special Week", "UMA-001", "Nutrición", "2026-12-01T15:45", "Programada", "Dra. Carla Ríos")
};

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// GET /api/citas  (requiere token) -> lista completa
app.MapGet("/api/citas", () => citas).RequireAuthorization();

// GET /api/citas/1  (requiere token) -> una cita o 404
app.MapGet("/api/citas/{id:int}", (int id) =>
    citas.FirstOrDefault(c => c.Id == id) is { } cita
        ? Results.Ok(cita)
        : Results.NotFound()).RequireAuthorization();

app.Run();

// Modelo propio de este microservicio (record local, sin dependencias compartidas).
record Cita(
    int Id,
    string Paciente,
    string HistoriaClinica,
    string Especialidad,
    string Fecha,
    string Estado,
    string Medico);

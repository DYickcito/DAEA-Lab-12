using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// GATEWAY: único punto de entrada. El cliente solo conoce http://localhost:5000;
// las rutas hacia cada microservicio se declaran en ocelot.json.
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// El gateway valida el token ANTES de reenviar la solicitud (misma clave que los servicios).
// "Bearer" es el nombre que ocelot.json usa en AuthenticationProviderKeys.
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

builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options => options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = emisor,
            ValidateAudience = true, ValidAudience = audiencia,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(clave))
        });

builder.Services.AddOcelot();

var app = builder.Build();

await app.UseOcelot();      // el middleware de Ocelot enruta y reenvía

app.Run();

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using UmaSalud.Datos;
using UmaSalud.Datos.Repositorios;
using UmaSalud.Negocio;
using UmaSalud.Negocio.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// OpenAPI con el esquema "Bearer": habilita el botón Authorize de Swagger.
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes =
            new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                }
            };
        document.Security = [new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        }];
        return Task.CompletedTask;
    }));

builder.Services.AddDbContext<ClinicaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Clinica")));
builder.Services.AddScoped<IEspecialidadRepositorio, EspecialidadRepositorio>();
builder.Services.AddScoped<IPacienteRepositorio, PacienteRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();   // S14
builder.Services.AddScoped<IEspecialidadServicio, EspecialidadServicio>();
builder.Services.AddScoped<IPacienteServicio, PacienteServicio>();
builder.Services.AddScoped<IAuthServicio, AuthServicio>();               // S14
builder.Services.AddAutoMapper(_ => { }, typeof(MappingProfile));
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins(builder.Configuration["Cors:Origin"] ?? "http://localhost:3000")
    .AllowAnyHeader().AllowAnyMethod()));

// JWT: los valores salen de la configuración (user-secrets o variables de entorno).
// La clave y la expiración NO se guardan en el repositorio.
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOpciones>() ?? new JwtOpciones();
if (string.IsNullOrWhiteSpace(jwt.Clave) || jwt.Clave.Length < 32)
    throw new InvalidOperationException(
        "Falta configurar Jwt:Clave (mínimo 32 caracteres). Use 'dotnet user-secrets set \"Jwt:Clave\" \"...\"' " +
        "o una variable de entorno como Jwt__Clave.");
if (string.IsNullOrWhiteSpace(jwt.Emisor) || string.IsNullOrWhiteSpace(jwt.Audiencia) || jwt.Minutos <= 0)
    throw new InvalidOperationException(
        "Configure Jwt:Emisor, Jwt:Audiencia y Jwt:Minutos (mayor que 0) en la configuración.");

builder.Services.AddSingleton(jwt);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = jwt.Emisor,
            ValidateAudience = true, ValidAudience = jwt.Audiencia,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.Clave))
        });
builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger: /swagger (con el botón Authorize para pegar el token)
app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Uma Salud v1"));
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("Frontend");
app.UseAuthentication();    // 1.º: lee y valida el token ("¿quién eres?")
app.UseAuthorization();     // 2.º: decide con [Authorize] y roles ("¿puedes hacerlo?")
app.MapControllers();
app.Run();

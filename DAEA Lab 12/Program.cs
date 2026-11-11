using Microsoft.EntityFrameworkCore;
using UmaSalud.Datos;
using UmaSalud.Datos.Repositorios;
using UmaSalud.Negocio;
using UmaSalud.Negocio.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ClinicaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Clinica")));
builder.Services.AddScoped<IEspecialidadRepositorio, EspecialidadRepositorio>();
builder.Services.AddScoped<IPacienteRepositorio, PacienteRepositorio>();
builder.Services.AddScoped<IEspecialidadServicio, EspecialidadServicio>();
builder.Services.AddScoped<IPacienteServicio, PacienteServicio>();
builder.Services.AddAutoMapper(_ => { }, typeof(MappingProfile));
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins(builder.Configuration["Cors:Origin"] ?? "http://localhost:3000")
    .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Uma Salud v1"));
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();
app.Run();

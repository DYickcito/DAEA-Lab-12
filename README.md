# Uma Salud (S14): API en capas protegida con JWT

Continúa la solución de la S13 (`ClinicaUma`, especialidades y pacientes) y agrega
registro, inicio de sesión y autorización por roles propios del negocio:
**Recepcionista** y **Medico**.

## Qué se agregó en cada capa

| Capa | Piezas nuevas |
|---|---|
| `UmaSalud.Entidades` | `Usuario`, `Roles`, `LoginDto`, `RegistroDto`, `TokenDto`, `PerfilDto` |
| `UmaSalud.Datos` | `IUsuarioRepositorio`, `UsuarioRepositorio` y `DbSet<Usuario>` en `ClinicaContext` |
| `UmaSalud.Negocio` | `AuthServicio` (BCrypt + generación del token), `IAuthServicio` y `JwtOpciones` |
| `DAEA Lab 12` (API) | `AuthController`, JwtBearer y Swagger con Authorize en `Program.cs`, y los `[Authorize]` de los controladores |

La tabla se crea con `Usuarios.sql`, que se ejecuta **después** de `ClinicaUma.sql`.

## Cómo ejecutarlo

1. En SQL Server, ejecuta `ClinicaUma.sql` y luego `Usuarios.sql`.
2. La cadena de conexión **tampoco está en el repositorio**: se configura con user-secrets
   (o la variable de entorno `ConnectionStrings__Clinica`). Ajusta el servidor si tu instancia
   no es `localhost`:

   ```powershell
   cd "DAEA Lab 12"
   dotnet user-secrets set "ConnectionStrings:Clinica" "Server=localhost;Database=ClinicaUma;Trusted_Connection=True;TrustServerCertificate=True;"
   ```

3. La clave JWT y la expiración **no están en el repositorio**: se configuran con user-secrets
   (el proyecto ya tiene `UserSecretsId`):

   ```powershell
   cd "DAEA Lab 12"
   dotnet user-secrets set "Jwt:Clave" "<clave aleatoria de al menos 32 caracteres>"
   dotnet user-secrets set "Jwt:Emisor" "UmaSaludApi"
   dotnet user-secrets set "Jwt:Audiencia" "UmaSalud.Clientes"
   dotnet user-secrets set "Jwt:Minutos" "60"
   ```

   También sirven variables de entorno (`Jwt__Clave`, `Jwt__Emisor`, `Jwt__Audiencia`, `Jwt__Minutos`).
   Si falta la clave o tiene menos de 32 caracteres, la API no arranca y muestra el motivo.
4. Ejecuta la API y abre `/swagger` (por defecto `http://localhost:5120/swagger`).

## Usuarios de prueba

| Correo | Clave | Rol |
|---|---|---|
| `recepcion@umasalud.com` | `Recepcion123!` | Recepcionista |
| `medico@umasalud.com` | `Medico123!` | Medico |

Los usuarios que se registran con `POST api/auth/registro` entran siempre como
**Recepcionista**: el rol lo decide el servidor, nunca el cliente (el DTO no tiene campo `Rol`).
Para promover uno a Medico se ejecuta el `UPDATE` comentado al final de `Usuarios.sql`.
Los hashes BCrypt de los usuarios de prueba se obtuvieron registrando las claves por la API.

## Matriz de permisos

| Endpoint | Sin token | Rol Recepcionista | Rol Medico |
|---|---|---|---|
| `POST /api/auth/registro` | Sí | Sí | Sí |
| `POST /api/auth/login` | Sí | Sí | Sí |
| `GET /api/auth/perfil` | No (401) | Sí | Sí |
| `GET /api/especialidades` | Sí | Sí | Sí |
| `GET /api/especialidades/{id}` | Sí | Sí | Sí |
| `GET /api/especialidades/{id}/pacientes` | No (401) | Sí | Sí |
| `POST /api/especialidades` | No (401) | No (403) | Sí |
| `PUT /api/especialidades/{id}` | No (401) | No (403) | Sí |
| `DELETE /api/especialidades/{id}` | No (401) | No (403) | Sí |
| `GET /api/pacientes` | No (401) | Sí | Sí |
| `GET /api/pacientes/{id}` | No (401) | Sí | Sí |
| `POST /api/pacientes` | No (401) | Sí | Sí |
| `PUT /api/pacientes/{id}` | No (401) | Sí | Sí |
| `DELETE /api/pacientes/{id}` | No (401) | No (403) | Sí |

Cómo se implementa:

- `[Authorize]` a nivel de controlador en `EspecialidadesController` y `PacientesController`:
  todo exige token salvo lo marcado con `[AllowAnonymous]`.
- `[AllowAnonymous]` en los GET públicos del catálogo y en registro/login.
- `[Authorize(Roles = Roles.Medico)]` en las altas, cambios y bajas de especialidades y en
  la baja de pacientes.

## Respuestas HTTP

| Resultado | Código | Dónde aparece |
|---|---|---|
| `Ok(datos)` | 200 | GET y login correctos |
| `CreatedAtAction` / `StatusCode(201)` | 201 | POST de especialidades y pacientes, y registro |
| `NoContent()` | 204 | PUT y DELETE correctos |
| `Unauthorized()` | 401 | Login inválido o endpoint protegido sin token |
| `Forbid()` de `[Authorize]` | 403 | Token válido sin el rol exigido |
| `Conflict(...)` | 409 | Correo repetido (regla de negocio) |

El error 401 del login es igual si el correo no existe o si la clave es incorrecta:
el servicio devuelve `null` en ambos casos para no revelar qué correo está registrado.

## Probar con Swagger

1. Ejecuta `POST /api/auth/login` con un usuario de prueba y copia el valor de `token`.
2. Pulsa **Authorize**, pega el token (sin la palabra `Bearer`) y confirma.
3. Ejecuta los endpoints protegidos: con el token del Medico pasan los de rol Medico;
   con el de Recepcionista responden 403.

También puedes usar `DAEA Lab 12.http` o Postman (Authorization → Bearer Token).

## Notas

- El claim del rol viaja con el nombre largo
  `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`; `[Authorize(Roles = ...)]`
  y `User.Claims` lo reconocen sin configuración extra.
- `GET /api/auth/perfil` devuelve correo y rol desde `User.Claims`, no consultando la base.
- Los paquetes usados son `BCrypt.Net-Next` 4.2.0 y `System.IdentityModel.Tokens.Jwt` 8.x en
  Negocio, y `Microsoft.AspNetCore.Authentication.JwtBearer` 10.x en la API.
- La clave JWT vive solo en user-secrets/variables de entorno; no se versiona.

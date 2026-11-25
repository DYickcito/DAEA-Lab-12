# S15: API Gateway con Ocelot - Uma Salud

Tres proyectos independientes. El cliente **solo** conoce `http://localhost:5000` (el gateway);
el gateway enruta hacia los microservicios, que escuchan en sus propios puertos.

| Proyecto | Puerto fijo | Rol |
|---|---|---|
| `GatewayApi` | 5000 | Único punto de entrada. Ocelot 25.x + JwtBearer 10.x. Rutas en `ocelot.json`. |
| `DAEA Lab 12` (Uma Salud) | 5120 | Microservicio 1: la API en capas ya desarrollada, sin cambios en su lógica. SQL Server. |
| `CitasApi` | 5121 | Microservicio 2: citas médicas con datos en memoria. Valida el mismo JWT. |

`CitasApi` no comparte base de datos ni clases con Uma Salud. Su modelo `Cita` es un `record`
local y los datos viven en una `List<Cita>`.

## Tabla de rutas

| Upstream (cliente → gateway) | Método | Downstream (gateway → servicio) | Token |
|---|---|---|---|
| `/gateway/auth/{everything}` | POST | `localhost:5120` `/api/auth/{everything}` | No |
| `/gateway/especialidades{everything}` | GET | `localhost:5120` `/api/especialidades{everything}` | No |
| `/gateway/pacientes{everything}` | GET | `localhost:5120` `/api/pacientes{everything}` | No (la API exige token para pacientes y responde 401) |
| `/gateway/especialidades{everything}` | POST, PUT, DELETE | `localhost:5120` `/api/especialidades{everything}` | Sí (gateway) |
| `/gateway/pacientes{everything}` | POST, PUT, DELETE | `localhost:5120` `/api/pacientes{everything}` | Sí (gateway) |
| `/gateway/citas{everything}` | GET | `localhost:5121` `/api/citas{everything}` | Sí (gateway) |

- El gateway valida el token con `AuthenticationOptions.AuthenticationProviderKeys = ["Bearer"]`
  antes de reenviar. El **rol** lo sigue exigiendo la API (por ejemplo, Recepcionista recibe 403
  al crear/eliminar especialidades y al eliminar pacientes).
- `{everything}` evita repetir una ruta por cada endpoint: cubre tanto `/gateway/citas` como
  `/gateway/citas/1`.

## Requisitos locales

1. SQL Server con autenticación de Windows (instancia `localhost`). Ejecuta los scripts en este orden:

   ```powershell
   sqlcmd -S localhost -E -b -f 65001 -i "DAEA Lab 12\ClinicaUma.sql"
   sqlcmd -S localhost -E -b -f 65001 -i "DAEA Lab 12\Usuarios.sql"
   ```

   El parámetro `-f 65001` es importante para que SQL Server guarde bien los acentos.
   Si ya ejecutaste los scripts sin él, corrige los textos con:

   ```powershell
   sqlcmd -S localhost -E -b -f 65001 -i "DAEA Lab 12\FixAcentos.sql"
   ```

   `ClinicaUma.sql` crea la base `ClinicaUma`, las tablas `especialidades` y `pacientes`, y datos
   de ejemplo. `Usuarios.sql` crea `usuarios` y siembra un usuario por rol.

2. La clave JWT y la cadena de conexión **no están en el repositorio**. Ya quedaron configuradas
   con user-secrets para esta máquina:

   ```powershell
   dotnet user-secrets set "Jwt:Clave"       "<misma clave de 32+ caracteres>" --project "DAEA Lab 12"
   dotnet user-secrets set "Jwt:Emisor"      "UmaSaludApi"                     --project "DAEA Lab 12"
   dotnet user-secrets set "Jwt:Audiencia"   "UmaSalud.Clientes"               --project "DAEA Lab 12"
   dotnet user-secrets set "Jwt:Minutos"     "60"                              --project "DAEA Lab 12"
   dotnet user-secrets set "ConnectionStrings:Clinica" "Server=localhost;Database=ClinicaUma;Trusted_Connection=True;TrustServerCertificate=True;" --project "DAEA Lab 12"
   ```

   Repite `Jwt:Clave`, `Jwt:Emisor` y `Jwt:Audiencia` con los **mismos valores** en `GatewayApi`
   y `CitasApi`. En producción se usan variables de entorno con el mismo nombre y doble guion bajo:

   ```text
   Jwt__Clave=...        Jwt__Emisor=UmaSaludApi        Jwt__Audiencia=UmaSalud.Clientes
   ConnectionStrings__Clinica=...
   ```

   Si falta la clave o la cadena de conexión, el proyecto no arranca y explica qué configurar.

3. Ejecuta los tres proyectos, cada uno en su terminal:

   ```powershell
   dotnet run --project "DAEA Lab 12"
   dotnet run --project CitasApi
   dotnet run --project GatewayApi
   ```

   En Visual Studio: clic derecho en la solución → **Configurar proyectos de inicio** →
   **Varios proyectos** → acción **Iniciar** para los tres.

4. Prueba todo con `Gateway.http` (o Postman/Thunder Client) usando solo `http://localhost:5000`.

## Usuarios de prueba

| Correo | Clave | Rol |
|---|---|---|
| `recepcion@umasalud.com` | `Recepcion123!` | Recepcionista |
| `medico@umasalud.com` | `Medico123!` | Medico |

## Respuestas esperadas

| Caso | Código |
|---|---|
| Login, GET públicos, citas con token | 200 |
| Registro o creación de paciente con token | 201 |
| Actualizar o eliminar con el rol correcto | 204 |
| Escritura sin token, o citas sin token (lo corta el gateway) | 401 |
| Escritura con Recepcionista donde la API exige Medico | 403 |
| Id inexistente o ruta/método ausente en `ocelot.json` | 404 |
| `CitasApi` detenido y se llama `/gateway/citas` | 502 |

## Pruebas realizadas localmente

- 200: `GET /gateway/especialidades`, `GET /gateway/citas` con token.
- 201: `POST /gateway/pacientes` con token.
- 204: `PUT /gateway/pacientes/{id}` con token.
- 401: `GET /gateway/citas` sin token y `GET http://localhost:5121/api/citas` directo sin token.
- 403: `POST /gateway/especialidades` con token de Recepcionista.
- 404: `GET /gateway/citas/999` y `GET /gateway/no-existe`.
- 502: `GET /gateway/citas` con token y `CitasApi` detenido.

# Uma Salud

API REST y página de consulta para un centro de salud de umamusumes. Usa ASP.NET Core 10, controladores, Entity Framework Core y **SQL Server**. La API trabaja directamente con el contexto y LINQ; la interfaz es una vista sencilla servida por la misma aplicación.

## Preparación local

1. Inicia SQL Server (Express, Developer o LocalDB) y abre SQL Server Management Studio con **autenticación de Windows**. El usuario de Windows necesita permisos para crear bases de datos.
2. Abre y ejecuta `DAEA Lab 12/ClinicaUma.sql` completo como UTF-8. Crea `ClinicaUma`, las tablas, la relación 1:N y los datos de ejemplo. Es un script de creación inicial: no lo vuelvas a ejecutar sobre una base existente. Para SQL Server remoto se necesita permiso de acceso de tu usuario Windows. En esta máquina ya se creó en `localhost` y se configuró el secreto local para esa instancia.
3. En la terminal, dentro de `DAEA Lab 12/`, configura la conexión mediante secretos locales (no se guarda en el repositorio). Ajusta el servidor a tu instalación, por ejemplo `.\SQLEXPRESS` o `(localdb)\MSSQLLocalDB`:

   ```powershell
   dotnet user-secrets set "ConnectionStrings:Clinica" "Server=.\SQLEXPRESS;Database=ClinicaUma;Trusted_Connection=True;TrustServerCertificate=True"
   dotnet run
   ```

   `TrustServerCertificate=True` está pensado para el certificado local de desarrollo. La identidad utilizada es la del usuario de Windows que ejecuta la aplicación. Si usas Visual Studio, establece el mismo secreto mediante **Administrar secretos de usuario** y ejecuta el proyecto.
4. Abre la URL mostrada por `dotnet run` para ver la página; accede a `/swagger` para probar todos los endpoints y a `/openapi/v1.json` para el documento OpenAPI. La política CORS permite por defecto únicamente `http://localhost:3000`; ajusta `Cors:Origin` mediante configuración si el cliente se sirve desde otro origen. La interfaz incluida usa el mismo origen y no necesita CORS.

## Database first y Scaffold

El esquema de referencia es el script SQL. Se ejecutó Scaffold contra la base local y se integró el resultado en `Models`, adaptando los nombres, las validaciones y la navegación nullable. Para reproducir la generación en una carpeta temporal mediante la consola del Administrador de paquetes NuGet de Visual Studio, con el proyecto como predeterminado:

```powershell
Scaffold-DbContext 'Server=.\SQLEXPRESS;Database=ClinicaUma;Trusted_Connection=True;TrustServerCertificate=True' Microsoft.EntityFrameworkCore.SqlServer -OutputDir ScaffoldTemporal -Context ClinicaScaffoldContext -NoOnConfiguring -DataAnnotations
```

Compara el resultado con `Models` antes de integrarlo: Scaffold genera nombres y mapeos según el esquema, y regenerar con `-Force` puede sobrescribir validaciones y ajustes. Al integrar el modelo generado, conserva las validaciones de longitud y rango, `Especialidad?`, `[JsonIgnore]` en la colección, el nombre de contexto usado en `Program.cs` y controladores, y no incluyas credenciales en el código. El comando anterior usa autenticación de Windows y no contiene contraseña.

## API

| Método | Ruta | Función |
|---|---|---|
| GET | `/api/especialidades` | Listar especialidades |
| GET | `/api/especialidades/{id}` | Consultar especialidad |
| GET | `/api/especialidades/{id}/pacientes` | Pacientes de una especialidad |
| POST | `/api/especialidades` | Registrar especialidad |
| PUT / DELETE | `/api/especialidades/{id}` | Actualizar / eliminar |
| GET | `/api/pacientes?nombre=Special&especialidadId=1` | Listar y filtrar |
| GET | `/api/pacientes/{id}` | Consultar paciente |
| POST | `/api/pacientes` | Registrar paciente |
| PUT / DELETE | `/api/pacientes/{id}` | Actualizar / eliminar |

Ejemplo para `POST /api/pacientes` (la navegación no es necesaria):

```json
{
  "nombre": "Mejiro McQueen",
  "historiaClinica": "UMA-005",
  "edad": 19,
  "motivoConsulta": "Control preventivo",
  "especialidadId": 1
}
```

Las acciones devuelven 200, 201, 204, 400, 404 o 409 según corresponda. Se rechazan con 409 nombres de especialidad o historias clínicas duplicados, y la eliminación de especialidades con pacientes. Los índices únicos y la clave foránea también protegen los datos ante solicitudes concurrentes. Las validaciones de datos se ejecutan automáticamente por `[ApiController]`.

**Aviso:** es una demostración académica sin autenticación ni autorización. No introducir datos médicos reales ni exponerla a internet sin medidas de seguridad adicionales. El despliegue y la base de datos en la nube se dejan para una fase posterior.

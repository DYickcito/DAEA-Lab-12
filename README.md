# Uma Salud

API REST y página de consulta para un centro de salud de umamusumes. Usa ASP.NET Core 10, Entity Framework Core, AutoMapper y **SQL Server**. La API expone DTOs; las entidades y el contexto quedan dentro de las capas internas.

## Arquitectura en capas

La solución `DAEA Lab 12.slnx` contiene cuatro proyectos, con referencias en cadena:

1. **DAEA Lab 12**: API ASP.NET Core; controladores, configuración y página web.
2. **UmaSalud.Negocio**: interfaces y servicios, reglas de negocio y `MappingProfile` de AutoMapper.
3. **UmaSalud.Datos**: `ClinicaContext`, interfaces e implementaciones de repositorios con consultas LINQ asíncronas.
4. **UmaSalud.Entidades**: entidades database-first y DTOs de lectura, creación y actualización.

La dirección de las referencias es API → Negocio → Datos → Entidades. Los controladores usan las interfaces de servicios y DTOs, no acceden al contexto ni devuelven entidades. Las entidades conservan la relación 1:N y no requieren `[JsonIgnore]`, porque no se serializan en las respuestas.

Los métodos de persistencia reales usan EF Core asíncrono; marcar una entidad para eliminación no requiere una operación de E/S y se completa en memoria antes de `SaveChangesAsync`.

## Preparación local

1. Inicia SQL Server (Express, Developer o LocalDB) y abre SQL Server Management Studio con **autenticación de Windows**. El usuario de Windows necesita permisos para crear bases de datos.
2. Si la base aún no existe, abre y ejecuta `DAEA Lab 12/ClinicaUma.sql` completo como UTF-8. Crea `ClinicaUma`, las tablas, la relación 1:N y los datos de ejemplo. Es un script de creación inicial: no lo vuelvas a ejecutar sobre una base existente. Para SQL Server remoto se necesita permiso de acceso de tu usuario Windows.
3. En la terminal, dentro de `DAEA Lab 12/`, configura la conexión mediante secretos locales (no se guarda en el repositorio). Ajusta el servidor a tu instalación, por ejemplo `.\SQLEXPRESS` o `(localdb)\MSSQLLocalDB`:

   ```powershell
   dotnet user-secrets set "ConnectionStrings:Clinica" "Server=.\SQLEXPRESS;Database=ClinicaUma;Trusted_Connection=True;TrustServerCertificate=True"
   dotnet run
   ```

   `TrustServerCertificate=True` está pensado para el certificado local de desarrollo. La identidad utilizada es la del usuario de Windows que ejecuta la aplicación. Si usas Visual Studio, establece el mismo secreto mediante **Administrar secretos de usuario** y ejecuta el proyecto.
4. Abre la URL mostrada por `dotnet run` para ver la página; accede a `/swagger` para probar todos los endpoints y a `/openapi/v1.json` para el documento OpenAPI. La política CORS permite por defecto únicamente `http://localhost:3000`; ajusta `Cors:Origin` mediante configuración si el cliente se sirve desde otro origen. La interfaz incluida usa el mismo origen y no necesita CORS.

## Database first y Scaffold

El esquema de referencia es el script SQL. El modelo procede del Scaffold realizado contra la base local y se integró y adaptó en `UmaSalud.Entidades/Models` y `UmaSalud.Datos/ClinicaContext.cs`. Para volver a comparar el esquema sin sobrescribir esas adaptaciones, selecciona el proyecto de API como predeterminado en la consola del Administrador de paquetes NuGet de Visual Studio y genera el resultado en una carpeta temporal:

```powershell
Scaffold-DbContext 'Server=.\SQLEXPRESS;Database=ClinicaUma;Trusted_Connection=True;TrustServerCertificate=True' Microsoft.EntityFrameworkCore.SqlServer -OutputDir ScaffoldTemporal -Context ClinicaScaffoldContext -NoOnConfiguring -DataAnnotations
```

Compara el resultado con ambas capas antes de integrarlo: regenerar con `-Force` puede sobrescribir ajustes. Conserva la navegación `Especialidad?`, las restricciones y la separación entre DTOs y entidades. No incluyas credenciales en el código. El comando anterior usa autenticación de Windows y no contiene contraseña. Las validaciones de entrada se aplican en los DTOs, no dependen del modelo generado.

## Sustentación de los mapeos

Todos los mapeos están en `UmaSalud.Negocio/MappingProfile.cs`. No se usa `ReverseMap`: cada dirección tiene un propósito distinto y los DTOs de entrada no deben asignar claves ni propiedades de navegación.

| Mapeo | Origen → destino | ¿Por qué en ese sentido? | Configuración especial |
|---|---|---|---|
| Especialidad → EspecialidadDto | Entidad → DTO de lectura | La API devuelve datos de la especialidad sin exponer la entidad. | Ninguna; los nombres coinciden. |
| EspecialidadCrearDto → Especialidad | DTO de creación → entidad | Convierte los datos recibidos en una entidad nueva para guardarla. | Se ignoran `Id` y `Pacientes`; la clave la asigna la base y no se reciben relaciones. |
| EspecialidadActualizarDto → Especialidad | DTO de actualización → entidad existente | Actualiza los campos permitidos sin crear otra entidad. | Se ignoran `Id` y `Pacientes`; el ID viene de la ruta y se conserva la relación. |
| Paciente → PacienteDto | Entidad → DTO de lectura | Expone los datos del paciente sin serializar su entidad relacionada. | `ForMember` y `MapFrom` obtienen `EspecialidadNombre` desde `Especialidad.Nombre`. |
| PacienteCrearDto → Paciente | DTO de creación → entidad | Convierte la entrada en una entidad nueva. | Se ignoran `Id` y `Especialidad`; el servicio valida y asigna la relación mediante `EspecialidadId`. |
| PacienteActualizarDto → Paciente | DTO de actualización → entidad existente | Actualiza los campos permitidos de un paciente existente. | Se ignoran `Id` y `Especialidad`; el servicio valida y asigna la relación. |

Los servicios usan `Map<TDestino>(origen)` al crear entidades y `Map(origen, destino)` para actualizar las existentes. Las reglas de duplicados y eliminación de especialidades con pacientes lanzan `ReglaNegocioException`, que los controladores traducen a HTTP 409. Una especialidad inexistente en la entrada de un paciente produce HTTP 400.

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

En las respuestas de pacientes, `especialidadNombre` contiene el nombre relacionado; ya no se devuelve el objeto anidado `especialidad`. Los DTOs de actualización toman el ID únicamente de la ruta; por ejemplo, `PUT /api/pacientes/5` recibe los mismos campos que el ejemplo de creación, sin necesitar `id` en el cuerpo. Esta versión requiere volver a publicarse para reemplazar cualquier despliegue anterior.

**Aviso:** es una demostración académica sin autenticación ni autorización. No introducir datos médicos reales ni exponerla a internet sin medidas de seguridad adicionales. El despliegue y la base de datos en la nube se dejan para una fase posterior.

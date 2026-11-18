-- S14: tabla de usuarios para autenticación JWT de Uma Salud.
-- Ejecutar DESPUÉS de ClinicaUma.sql (S13). No lo ejecute sobre datos existentes:
-- el script elimina y vuelve a crear la tabla.
USE ClinicaUma;
GO

IF OBJECT_ID('dbo.usuarios', 'U') IS NOT NULL DROP TABLE dbo.usuarios;
GO

CREATE TABLE dbo.usuarios (
    id         int IDENTITY(1,1) NOT NULL CONSTRAINT PK_usuarios PRIMARY KEY,
    correo     nvarchar(100) NOT NULL,
    clave_hash nvarchar(100) NOT NULL,   -- hash BCrypt, nunca la clave
    rol        nvarchar(20)  NOT NULL,
    CONSTRAINT UQ_usuarios_correo UNIQUE (correo),
    CONSTRAINT CK_usuarios_rol CHECK (rol IN (N'Recepcionista', N'Medico'))
);
GO

-- Un usuario de prueba por cada rol. Las claves de prueba son:
--   recepcion@umasalud.com / Recepcion123!   (rol Recepcionista)
--   medico@umasalud.com    / Medico123!     (rol Medico)
-- El hash se obtuvo registrando la clave por la API (BCrypt) y luego
-- asignando el rol con un UPDATE; aquí queda fijo para poder sembrar la tabla.
INSERT INTO dbo.usuarios (correo, clave_hash, rol) VALUES
(N'recepcion@umasalud.com', N'$2a$11$Q9L7QLBJ/EU2Ss5ILrs2m.5CZ1w0SAt8uy67Ys0fxR7zEMfcaTg6C', N'Recepcionista'),
(N'medico@umasalud.com',    N'$2a$11$oBTrWRH9AMtFxX/n/aEabe5eAJ5BL65/kPpST0NXliVzg8WlOZPBa', N'Medico');
GO

-- Los usuarios que se registran por POST api/auth/registro entran como Recepcionista.
-- Para promoverlos a Medico, el administrador de la base ejecuta:
--   UPDATE dbo.usuarios SET rol = N'Medico' WHERE correo = N'correo@ejemplo.com';
SELECT id, correo, rol FROM dbo.usuarios;
GO

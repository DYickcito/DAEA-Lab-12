-- Ejecutar en SQL Server Management Studio conectado con autenticación de Windows.
-- El script crea la base de datos una sola vez; no lo ejecute sobre datos existentes.
CREATE DATABASE ClinicaUma;
GO
USE ClinicaUma;
GO
CREATE TABLE dbo.especialidades (
    id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_especialidades PRIMARY KEY,
    nombre nvarchar(80) NOT NULL,
    descripcion nvarchar(250) NOT NULL,
    CONSTRAINT UQ_especialidades_nombre UNIQUE (nombre)
);
CREATE TABLE dbo.pacientes (
    id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_pacientes PRIMARY KEY,
    nombre nvarchar(100) NOT NULL,
    historia_clinica nvarchar(20) NOT NULL,
    edad int NOT NULL CONSTRAINT CK_pacientes_edad CHECK (edad BETWEEN 1 AND 120),
    motivo_consulta nvarchar(200) NOT NULL,
    especialidad_id int NOT NULL,
    CONSTRAINT UQ_pacientes_historia UNIQUE (historia_clinica),
    CONSTRAINT FK_pacientes_especialidades FOREIGN KEY (especialidad_id)
        REFERENCES dbo.especialidades(id) ON DELETE NO ACTION
);
GO
INSERT INTO dbo.especialidades (nombre, descripcion) VALUES
(N'Medicina general', N'Atención integral y controles de salud.'),
(N'Medicina deportiva', N'Prevención y seguimiento de lesiones durante el entrenamiento.'),
(N'Nutrición', N'Planes de alimentación para el bienestar y rendimiento.');
INSERT INTO dbo.pacientes (nombre, historia_clinica, edad, motivo_consulta, especialidad_id) VALUES
(N'Special Week', N'UMA-001', 18, N'Control general de salud', 1),
(N'Silence Suzuka', N'UMA-002', 19, N'Seguimiento de entrenamiento', 2),
(N'Tokai Teio', N'UMA-003', 18, N'Revisión preventiva de lesiones', 2),
(N'Oguri Cap', N'UMA-004', 20, N'Asesoría nutricional', 3);
GO

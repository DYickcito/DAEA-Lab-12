-- Corrige los acentos que se guardaron mal al ejecutar ClinicaUma.sql sin -f 65001.
USE ClinicaUma;
GO

UPDATE dbo.especialidades
SET descripcion = N'Atención integral y controles de salud.'
WHERE id = 1;

UPDATE dbo.especialidades
SET descripcion = N'Prevención y seguimiento de lesiones durante el entrenamiento.'
WHERE id = 2;

UPDATE dbo.especialidades
SET nombre = N'Nutrición',
    descripcion = N'Planes de alimentación para el bienestar y rendimiento.'
WHERE id = 3;

UPDATE dbo.pacientes
SET motivo_consulta = N'Revisión preventiva de lesiones'
WHERE id = 3;

UPDATE dbo.pacientes
SET motivo_consulta = N'Asesoría nutricional'
WHERE id = 4;

SELECT id, nombre, descripcion FROM dbo.especialidades ORDER BY id;
SELECT id, nombre, motivo_consulta FROM dbo.pacientes ORDER BY id;
GO

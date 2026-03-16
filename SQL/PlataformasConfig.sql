-- ============================================================
-- Migración: Tabla PlataformasConfig
-- Fecha: 2026-03-12
-- Configura los campos de credenciales por tipo de plataforma.
-- Para agregar/modificar plataformas usar el endpoint:
--   POST/PUT /api/plataformas-config
-- ============================================================

CREATE TABLE [dbo].[PlataformasConfig]
(
    Id             INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Plataforma     NVARCHAR(50)  NOT NULL UNIQUE,
    NombreAmigable NVARCHAR(100) NOT NULL,
    LabelUsuario   NVARCHAR(50)  NOT NULL DEFAULT 'Usuario',  -- 'Usuario' | 'Correo'
    TieneUrl       BIT           NOT NULL DEFAULT 0,
    TienePin       BIT           NOT NULL DEFAULT 0,
    IsActive       BIT           NOT NULL DEFAULT 1
);
GO

-- Datos iniciales
-- IPTV y FlujoTV: usuario + contraseña
-- Netflix y Streaming: correo + contraseña + PIN
INSERT INTO [dbo].[PlataformasConfig] (Plataforma, NombreAmigable, LabelUsuario, TieneUrl, TienePin) VALUES
    ('IPTV',      'IPTV',      'Usuario', 0, 0),
    ('FlujoTV',   'Flujo TV',  'Usuario', 0, 0),
    ('Netflix',   'Netflix',   'Correo',  0, 1),
    ('Streaming', 'Streaming', 'Correo',  0, 1);
GO

-- ============================================================
-- SP: Obtener todas las configuraciones activas
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PlataformasConfig_ObtenerTodas]
AS
BEGIN
    SELECT Id, Plataforma, NombreAmigable, LabelUsuario, TieneUrl, TienePin, IsActive
    FROM   [dbo].[PlataformasConfig]
    WHERE  IsActive = 1
    ORDER BY Id;
END;
GO

-- ============================================================
-- SP: Obtener por Id
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PlataformasConfig_ObtenerPorId]
    @Id INT
AS
BEGIN
    SELECT Id, Plataforma, NombreAmigable, LabelUsuario, TieneUrl, TienePin, IsActive
    FROM   [dbo].[PlataformasConfig]
    WHERE  Id = @Id;
END;
GO

-- ============================================================
-- SP: Crear nueva plataforma
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PlataformasConfig_Crear]
    @Plataforma     NVARCHAR(50),
    @NombreAmigable NVARCHAR(100),
    @LabelUsuario   NVARCHAR(50) = 'Usuario',
    @TieneUrl       BIT          = 0,
    @TienePin       BIT          = 0,
    @NewId          INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[PlataformasConfig] (Plataforma, NombreAmigable, LabelUsuario, TieneUrl, TienePin)
    VALUES (@Plataforma, @NombreAmigable, @LabelUsuario, @TieneUrl, @TienePin);
    SET @NewId = SCOPE_IDENTITY();
END;
GO

-- ============================================================
-- SP: Actualizar plataforma existente
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PlataformasConfig_Actualizar]
    @Id             INT,
    @Plataforma     NVARCHAR(50),
    @NombreAmigable NVARCHAR(100),
    @LabelUsuario   NVARCHAR(50),
    @TieneUrl       BIT,
    @TienePin       BIT
AS
BEGIN
    UPDATE [dbo].[PlataformasConfig]
    SET Plataforma     = @Plataforma,
        NombreAmigable = @NombreAmigable,
        LabelUsuario   = @LabelUsuario,
        TieneUrl       = @TieneUrl,
        TienePin       = @TienePin
    WHERE Id = @Id;
END;
GO

-- ============================================================
-- SP: Desactivar plataforma (soft-delete)
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PlataformasConfig_Desactivar]
    @Id INT
AS
BEGIN
    UPDATE [dbo].[PlataformasConfig] SET IsActive = 0 WHERE Id = @Id;
END;
GO

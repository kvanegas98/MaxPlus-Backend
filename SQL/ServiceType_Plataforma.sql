-- ============================================================
-- Migración: Agregar columna Plataforma a TiposServicio
-- Fecha: 2026-03-12
-- Valores: IPTV | FlujoTV | Netflix | Streaming
--   IPTV      → Usuario + Contraseña + URL
--   FlujoTV   → Usuario + Contraseña
--   Netflix   → Correo + Contraseña + PIN
--   Streaming → Correo + Contraseña (HBO, Disney, Vix, Paramount, YouTube, Prime)
-- ============================================================

ALTER TABLE [dbo].[TiposServicio]
ADD Plataforma NVARCHAR(50) NOT NULL DEFAULT 'IPTV';
GO

-- Asignar plataforma por ID exacto
UPDATE [dbo].[TiposServicio] SET Plataforma = 'IPTV'      WHERE Id = '7B78FF51-90E0-4E0C-8986-B723532A503D'; -- 1 Perfil IPTV
UPDATE [dbo].[TiposServicio] SET Plataforma = 'IPTV'      WHERE Id = '44B20338-FA62-43F2-931D-E3A4FA86266F'; -- Demo gratuita 4 Horas
UPDATE [dbo].[TiposServicio] SET Plataforma = 'FlujoTV'   WHERE Id = '44A93188-F862-4A8C-9EB0-6B90F4CA1602'; -- 1 Perfil Flujotv
UPDATE [dbo].[TiposServicio] SET Plataforma = 'Netflix'   WHERE Id = 'DB6A411B-A6C9-45CA-B6B1-E7031CF8F606'; -- 1 Perfil Netflix
UPDATE [dbo].[TiposServicio] SET Plataforma = 'Streaming' WHERE Id = '9E61BA79-FA4F-4788-810F-2D71FC433F69'; -- 1 Perfil HBO Max
UPDATE [dbo].[TiposServicio] SET Plataforma = 'Streaming' WHERE Id = 'B81032CA-C8BB-4204-BD89-30E8DD987783'; -- 1 Perfil Disney Premium
UPDATE [dbo].[TiposServicio] SET Plataforma = 'Streaming' WHERE Id = '6F1B899C-8DC8-4988-B21E-776D2FE45AB4'; -- 1 Perfil Vix+
UPDATE [dbo].[TiposServicio] SET Plataforma = 'Streaming' WHERE Id = '09D2F854-58A0-4773-9C5C-8C9C9DCBB6D9'; -- 1 Perfil Paramount
UPDATE [dbo].[TiposServicio] SET Plataforma = 'Streaming' WHERE Id = '34062836-B71F-40BE-8A97-AD017551DCEF'; -- 1 Perfil YouTube Premium
UPDATE [dbo].[TiposServicio] SET Plataforma = 'Streaming' WHERE Id = '46963B32-9A82-4966-8F63-E14B79F2DB8C'; -- 1 Perfil Prime Video
GO

-- ============================================================
-- Actualizar SP: sp_TiposServicio_ObtenerTodos (con aliases para Dapper)
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_ObtenerTodos]
AS
BEGIN
    SELECT Id,
           Nombre       AS Name,
           Descripcion  AS Description,
           Precio       AS Price,
           PrecioCompra AS PurchasePrice,
           DurationDays,
           Category,
           Plataforma,
           ImageUrl,
           IsActive,
           CreatedAt
    FROM [dbo].[TiposServicio]
    WHERE IsActive = 1
    ORDER BY Nombre;
END;
GO

-- ============================================================
-- Actualizar SP: sp_TiposServicio_ObtenerPorId (con aliases para Dapper)
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_ObtenerPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SELECT Id,
           Nombre       AS Name,
           Descripcion  AS Description,
           Precio       AS Price,
           PrecioCompra AS PurchasePrice,
           DurationDays,
           Category,
           Plataforma,
           ImageUrl,
           IsActive,
           CreatedAt
    FROM [dbo].[TiposServicio]
    WHERE Id = @Id;
END;
GO

-- ============================================================
-- Actualizar SP: sp_TiposServicio_Crear
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_Crear]
    @Nombre       NVARCHAR(150),
    @Descripcion  NVARCHAR(500),
    @Precio       DECIMAL(18,2),
    @PrecioCompra DECIMAL(18,2),
    @DurationDays INT,
    @Category     NVARCHAR(50),
    @Plataforma   NVARCHAR(50) = 'IPTV',
    @ImageUrl     NVARCHAR(500) = NULL,
    @Id           UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();
    INSERT INTO [dbo].[TiposServicio] (Id, Nombre, Descripcion, Precio, PrecioCompra, DurationDays, Category, Plataforma, ImageUrl)
    VALUES (@Id, @Nombre, @Descripcion, @Precio, @PrecioCompra, @DurationDays, @Category, @Plataforma, @ImageUrl);
END;
GO

-- ============================================================
-- Actualizar SP: sp_TiposServicio_Actualizar
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_Actualizar]
    @Id           UNIQUEIDENTIFIER,
    @Nombre       NVARCHAR(150),
    @Descripcion  NVARCHAR(500),
    @Precio       DECIMAL(18,2),
    @PrecioCompra DECIMAL(18,2),
    @DurationDays INT,
    @Category     NVARCHAR(50),
    @Plataforma   NVARCHAR(50) = 'IPTV',
    @ImageUrl     NVARCHAR(500) = NULL
AS
BEGIN
    UPDATE [dbo].[TiposServicio]
    SET Nombre       = @Nombre,
        Descripcion  = @Descripcion,
        Precio       = @Precio,
        PrecioCompra = @PrecioCompra,
        DurationDays = @DurationDays,
        Category     = @Category,
        Plataforma   = @Plataforma,
        ImageUrl     = @ImageUrl
    WHERE Id = @Id;
END;
GO

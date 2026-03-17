-- ============================================================
-- Categorías dinámicas para TiposServicio
-- Fecha: 2026-03-16
-- ============================================================

-- 1. Crear tabla Categorias
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('dbo.Categorias') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Categorias] (
        [Id]          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Categorias_Id DEFAULT NEWID() PRIMARY KEY,
        [Nombre]      NVARCHAR(100)    NOT NULL,
        [Descripcion] NVARCHAR(300)    NULL,
        [Color]       NVARCHAR(7)      NULL DEFAULT '#8B5CF6',
        [Orden]       INT              NOT NULL DEFAULT 0,
        [IsActive]    BIT              NOT NULL DEFAULT 1,
        [CreatedAt]   DATETIME2        NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 2. Agregar CategoriaId a TiposServicio
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.TiposServicio') AND name = 'CategoriaId'
)
BEGIN
    ALTER TABLE [dbo].[TiposServicio]
    ADD CategoriaId UNIQUEIDENTIFIER NULL
    CONSTRAINT FK_TiposServicio_Categorias FOREIGN KEY REFERENCES [dbo].[Categorias](Id);
END
GO

-- 3. Insertar categorías base y migrar servicios existentes
DECLARE @IdPaid UNIQUEIDENTIFIER = NEWID();
DECLARE @IdDemo UNIQUEIDENTIFIER = NEWID();
DECLARE @IdOtro UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Categorias] (Id, Nombre, Descripcion, Color, Orden)
VALUES
    (@IdPaid, 'Servicios',  'Planes de pago mensuales',  '#8B5CF6', 1),
    (@IdDemo, 'Demos',      'Pruebas gratuitas',          '#10B981', 2),
    (@IdOtro, 'Otros',      'Servicios adicionales',      '#FF6B00', 3);

-- Migrar servicios existentes según el campo Category actual
UPDATE [dbo].[TiposServicio] SET CategoriaId = @IdPaid WHERE Category = 'Paid'  AND CategoriaId IS NULL;
UPDATE [dbo].[TiposServicio] SET CategoriaId = @IdDemo WHERE Category = 'Demo'  AND CategoriaId IS NULL;
UPDATE [dbo].[TiposServicio] SET CategoriaId = @IdOtro WHERE CategoriaId IS NULL;
GO

-- ============================================================
-- SPs de Categorias
-- ============================================================

CREATE OR ALTER PROCEDURE [dbo].[sp_Categorias_ObtenerTodos]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Descripcion, Color, Orden, IsActive, CreatedAt
    FROM [dbo].[Categorias]
    ORDER BY Orden, Nombre;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Categorias_ObtenerActivas]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Descripcion, Color, Orden, CreatedAt
    FROM [dbo].[Categorias]
    WHERE IsActive = 1
    ORDER BY Orden, Nombre;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Categorias_ObtenerPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Descripcion, Color, Orden, IsActive, CreatedAt
    FROM [dbo].[Categorias]
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Categorias_Crear]
    @Nombre      NVARCHAR(100),
    @Descripcion NVARCHAR(300) = NULL,
    @Color       NVARCHAR(7)   = '#8B5CF6',
    @Orden       INT           = 0,
    @Id          UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();
    INSERT INTO [dbo].[Categorias] (Id, Nombre, Descripcion, Color, Orden)
    VALUES (@Id, @Nombre, @Descripcion, @Color, @Orden);
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Categorias_Actualizar]
    @Id          UNIQUEIDENTIFIER,
    @Nombre      NVARCHAR(100),
    @Descripcion NVARCHAR(300) = NULL,
    @Color       NVARCHAR(7)   = '#8B5CF6',
    @Orden       INT           = 0,
    @IsActive    BIT           = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Categorias]
    SET Nombre      = @Nombre,
        Descripcion = @Descripcion,
        Color       = @Color,
        Orden       = @Orden,
        IsActive    = @IsActive
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Categorias_Eliminar]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    -- Desasignar servicios antes de eliminar
    UPDATE [dbo].[TiposServicio] SET CategoriaId = NULL WHERE CategoriaId = @Id;
    DELETE FROM [dbo].[Categorias] WHERE Id = @Id;
END
GO

-- ============================================================
-- Actualizar sp_TiposServicio_ObtenerTodos — JOIN con Categorias
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_ObtenerTodos]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        t.Id,
        t.Nombre        AS Name,
        t.Descripcion   AS Description,
        t.Precio        AS Price,
        t.PrecioCompra  AS PurchasePrice,
        t.DurationDays,
        t.Category,
        t.Plataforma,
        t.ImageUrl,
        t.IsActive,
        t.CreatedAt,
        t.CategoriaId,
        c.Nombre        AS CategoriaNombre,
        c.Color         AS CategoriaColor
    FROM [dbo].[TiposServicio] t
    LEFT JOIN [dbo].[Categorias] c ON c.Id = t.CategoriaId
    WHERE t.IsDeleted = 0
    ORDER BY c.Orden, t.Nombre;
END
GO

-- ============================================================
-- Actualizar sp_TiposServicio_ObtenerCatalogo — público, solo activos
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_ObtenerCatalogo]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        t.Id,
        t.Nombre        AS Name,
        t.Descripcion   AS Description,
        t.Precio        AS Price,
        t.DurationDays,
        t.Category,
        t.Plataforma,
        t.ImageUrl,
        t.CategoriaId,
        c.Nombre        AS CategoriaNombre,
        c.Color         AS CategoriaColor,
        c.Orden         AS CategoriaOrden
    FROM [dbo].[TiposServicio] t
    LEFT JOIN [dbo].[Categorias] c ON c.Id = t.CategoriaId
    WHERE t.IsActive  = 1
      AND t.IsDeleted = 0
    ORDER BY c.Orden, t.Nombre;
END
GO

-- ============================================================
-- Actualizar sp_TiposServicio_ObtenerPorId
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_ObtenerPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        t.Id,
        t.Nombre        AS Name,
        t.Descripcion   AS Description,
        t.Precio        AS Price,
        t.PrecioCompra  AS PurchasePrice,
        t.DurationDays,
        t.Category,
        t.Plataforma,
        t.ImageUrl,
        t.IsActive,
        t.CreatedAt,
        t.CategoriaId,
        c.Nombre        AS CategoriaNombre,
        c.Color         AS CategoriaColor
    FROM [dbo].[TiposServicio] t
    LEFT JOIN [dbo].[Categorias] c ON c.Id = t.CategoriaId
    WHERE t.Id = @Id;
END
GO

-- ============================================================
-- Actualizar sp_TiposServicio_Crear — acepta CategoriaId
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_Crear]
    @Nombre       NVARCHAR(150),
    @Descripcion  NVARCHAR(500)    = NULL,
    @Precio       DECIMAL(18,2),
    @PrecioCompra DECIMAL(18,2),
    @DurationDays INT,
    @Category     NVARCHAR(50)     = 'Paid',
    @Plataforma   NVARCHAR(50)     = 'IPTV',
    @ImageUrl     NVARCHAR(500)    = NULL,
    @CategoriaId  UNIQUEIDENTIFIER = NULL,
    @Id           UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();
    INSERT INTO [dbo].[TiposServicio]
        (Id, Nombre, Descripcion, Precio, PrecioCompra, DurationDays, Category, Plataforma, ImageUrl, CategoriaId)
    VALUES
        (@Id, @Nombre, @Descripcion, @Precio, @PrecioCompra, @DurationDays, @Category, @Plataforma, @ImageUrl, @CategoriaId);
END
GO

-- ============================================================
-- Actualizar sp_TiposServicio_Actualizar — acepta CategoriaId
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_Actualizar]
    @Id           UNIQUEIDENTIFIER,
    @Nombre       NVARCHAR(150),
    @Descripcion  NVARCHAR(500)    = NULL,
    @Precio       DECIMAL(18,2),
    @PrecioCompra DECIMAL(18,2),
    @DurationDays INT,
    @Category     NVARCHAR(50)     = 'Paid',
    @Plataforma   NVARCHAR(50)     = 'IPTV',
    @ImageUrl     NVARCHAR(500)    = NULL,
    @IsActive     BIT              = 1,
    @CategoriaId  UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[TiposServicio]
    SET Nombre       = @Nombre,
        Descripcion  = @Descripcion,
        Precio       = @Precio,
        PrecioCompra = @PrecioCompra,
        DurationDays = @DurationDays,
        Category     = @Category,
        Plataforma   = @Plataforma,
        ImageUrl     = @ImageUrl,
        IsActive     = @IsActive,
        CategoriaId  = @CategoriaId
    WHERE Id = @Id;
END
GO

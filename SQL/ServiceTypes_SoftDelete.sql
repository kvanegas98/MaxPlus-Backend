-- ============================================================
-- Actualiza SPs de TiposServicio para soportar IsDeleted (soft delete)
-- y los campos IsActive, Plataforma
-- ============================================================

-- ObtenerTodos: excluye eliminados
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_ObtenerTodos]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM [dbo].[TiposServicio]
    WHERE IsDeleted = 0
    ORDER BY Nombre;
END;
GO

-- ObtenerPorId: excluye eliminados
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_ObtenerPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM [dbo].[TiposServicio]
    WHERE Id = @Id AND IsDeleted = 0;
END;
GO

-- Crear: incluye Plataforma
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_Crear]
    @Nombre       NVARCHAR(150),
    @Descripcion  NVARCHAR(500)  = NULL,
    @Precio       DECIMAL(18,2),
    @PrecioCompra DECIMAL(18,2),
    @DurationDays INT,
    @Category     NVARCHAR(50),
    @Plataforma   NVARCHAR(50)   = 'IPTV',
    @ImageUrl     NVARCHAR(500)  = NULL,
    @Id           UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();
    INSERT INTO [dbo].[TiposServicio]
        (Id, Nombre, Descripcion, Precio, PrecioCompra, DurationDays, Category, Plataforma, ImageUrl)
    VALUES
        (@Id, @Nombre, @Descripcion, @Precio, @PrecioCompra, @DurationDays, @Category, @Plataforma, @ImageUrl);
END;
GO

-- Actualizar: incluye Plataforma, IsActive
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_Actualizar]
    @Id           UNIQUEIDENTIFIER,
    @Nombre       NVARCHAR(150),
    @Descripcion  NVARCHAR(500)  = NULL,
    @Precio       DECIMAL(18,2),
    @PrecioCompra DECIMAL(18,2),
    @DurationDays INT,
    @Category     NVARCHAR(50),
    @Plataforma   NVARCHAR(50)   = 'IPTV',
    @ImageUrl     NVARCHAR(500)  = NULL,
    @IsActive     BIT            = 1
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
        IsActive     = @IsActive
    WHERE Id = @Id AND IsDeleted = 0;
END;
GO

-- Eliminar (soft delete): marca IsDeleted = 1
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_Eliminar]
    @Id        UNIQUEIDENTIFIER,
    @DeletedBy NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[TiposServicio]
    SET IsDeleted = 1,
        DeletedAt = GETDATE(),
        DeletedBy = @DeletedBy,
        IsActive  = 0
    WHERE Id = @Id;
END;
GO

-- Desactivar: solo cambia IsActive (el servicio sigue existiendo)
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_Desactivar]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[TiposServicio]
    SET IsActive = 0
    WHERE Id = @Id AND IsDeleted = 0;
END;
GO

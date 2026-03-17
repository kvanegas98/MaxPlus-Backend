-- ============================================================
-- Actualiza sp_TiposServicio_Actualizar para incluir IsActive y Plataforma
-- ============================================================
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
    WHERE Id = @Id;
END;
GO

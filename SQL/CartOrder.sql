-- ============================================================
-- Carrito de compras — ServiceOrderItems
-- SEGURO: no modifica tablas existentes, solo agrega
-- Fecha: 2026-03-17
-- ============================================================

-- 1. Crear tabla ServiceOrderItems
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('dbo.ServiceOrderItems') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[ServiceOrderItems] (
        [Id]             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ServiceOrderItems_Id DEFAULT NEWID() PRIMARY KEY,
        [ServiceOrderId] UNIQUEIDENTIFIER NOT NULL,
        [TipoServicioId] UNIQUEIDENTIFIER NOT NULL,
        [DurationMonths] INT              NOT NULL DEFAULT 1,
        [Cantidad]       INT              NOT NULL DEFAULT 1,
        [UnitPrice]      DECIMAL(18,2)   NOT NULL DEFAULT 0,
        [SubTotal]       DECIMAL(18,2)   NOT NULL DEFAULT 0,
        [SubscriptionId] UNIQUEIDENTIFIER NULL,
        [CreatedAt]      DATETIME2        NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_ServiceOrderItems_Order    FOREIGN KEY ([ServiceOrderId]) REFERENCES [dbo].[ServiceOrders]([Id]),
        CONSTRAINT FK_ServiceOrderItems_Servicio FOREIGN KEY ([TipoServicioId]) REFERENCES [dbo].[TiposServicio]([Id])
    );
END
GO

-- ============================================================
-- SPs de ServiceOrderItems
-- ============================================================

CREATE OR ALTER PROCEDURE [dbo].[sp_ServiceOrderItems_Crear]
    @ServiceOrderId UNIQUEIDENTIFIER,
    @TipoServicioId UNIQUEIDENTIFIER,
    @DurationMonths INT            = 1,
    @Cantidad       INT            = 1,
    @UnitPrice      DECIMAL(18,2)  = 0,
    @SubTotal       DECIMAL(18,2)  = 0,
    @Id             UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();
    INSERT INTO [dbo].[ServiceOrderItems]
        (Id, ServiceOrderId, TipoServicioId, DurationMonths, Cantidad, UnitPrice, SubTotal)
    VALUES
        (@Id, @ServiceOrderId, @TipoServicioId, @DurationMonths, @Cantidad, @UnitPrice, @SubTotal);
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ServiceOrderItems_ObtenerPorOrden]
    @ServiceOrderId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        i.Id,
        i.ServiceOrderId,
        i.TipoServicioId,
        i.DurationMonths,
        i.Cantidad,
        i.UnitPrice,
        i.SubTotal,
        i.SubscriptionId,
        i.CreatedAt,
        t.Nombre     AS ServiceName,
        t.Plataforma,
        t.ImageUrl
    FROM [dbo].[ServiceOrderItems] i
    INNER JOIN [dbo].[TiposServicio] t ON t.Id = i.TipoServicioId
    WHERE i.ServiceOrderId = @ServiceOrderId
    ORDER BY i.CreatedAt;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ServiceOrderItems_AsignarSuscripcion]
    @Id             UNIQUEIDENTIFIER,
    @SubscriptionId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[ServiceOrderItems]
    SET SubscriptionId = @SubscriptionId
    WHERE Id = @Id;
END
GO

-- ============================================================
-- Actualizar sp_ServiceOrders_Crear para que TipoServicioId sea opcional
-- (las órdenes antiguas siguen funcionando, las del carrito no lo necesitan)
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ServiceOrders_Crear]
    @CustomerName   NVARCHAR(150),
    @CustomerPhone  NVARCHAR(20)     = NULL,
    @CustomerEmail  NVARCHAR(100)    = NULL,
    @TipoServicioId UNIQUEIDENTIFIER = NULL,
    @Notes          NVARCHAR(500)    = NULL,
    @IpAddress      NVARCHAR(45)     = NULL,
    @Id             UNIQUEIDENTIFIER OUTPUT,
    @NumeroOrden    NVARCHAR(20)     OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();
    SET @NumeroOrden = 'ORD-' + UPPER(LEFT(REPLACE(CAST(@Id AS NVARCHAR(36)), '-', ''), 8));

    INSERT INTO [dbo].[ServiceOrders]
        (Id, NumeroOrden, CustomerName, CustomerPhone, CustomerEmail, TipoServicioId, Notes, IpAddress)
    VALUES
        (@Id, @NumeroOrden, @CustomerName, @CustomerPhone, @CustomerEmail, @TipoServicioId, @Notes, @IpAddress);
END
GO

-- ============================================================
-- Actualizar sp_ServiceOrders_ObtenerPorId para incluir items (count)
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ServiceOrders_ObtenerPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        o.Id,
        o.NumeroOrden,
        o.CustomerName,
        o.CustomerPhone,
        o.CustomerEmail,
        o.TipoServicioId,
        o.Notes,
        o.Status,
        o.ApprovedBy,
        o.ApprovedAt,
        o.SubscriptionId,
        o.RejectionReason,
        o.IpAddress,
        o.CreatedAt,
        t.Nombre AS ServiceName,
        (SELECT COUNT(*) FROM [dbo].[ServiceOrderItems] i WHERE i.ServiceOrderId = o.Id) AS ItemCount
    FROM [dbo].[ServiceOrders] o
    LEFT JOIN [dbo].[TiposServicio] t ON t.Id = o.TipoServicioId
    WHERE o.Id = @Id;
END
GO

-- ============================================================
-- Actualizar sp_ServiceOrders_ObtenerTodos para incluir ItemCount
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ServiceOrders_ObtenerTodos]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        o.Id,
        o.NumeroOrden,
        o.CustomerName,
        o.CustomerPhone,
        o.CustomerEmail,
        o.TipoServicioId,
        o.Notes,
        o.Status,
        o.ApprovedBy,
        o.ApprovedAt,
        o.SubscriptionId,
        o.RejectionReason,
        o.IpAddress,
        o.CreatedAt,
        t.Nombre AS ServiceName,
        (SELECT COUNT(*) FROM [dbo].[ServiceOrderItems] i WHERE i.ServiceOrderId = o.Id) AS ItemCount
    FROM [dbo].[ServiceOrders] o
    LEFT JOIN [dbo].[TiposServicio] t ON t.Id = o.TipoServicioId
    WHERE (@Status IS NULL OR o.Status = @Status)
    ORDER BY o.CreatedAt DESC;
END
GO

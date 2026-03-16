-- ============================================================
-- Migración: sp_ObtenerFacturaPorId incluye datos del MetodoPago
-- Fecha: 2026-03-13
-- ============================================================

-- 1. Agregar columnas MetodoPagoId y PaymentReference si no existen
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Facturas') AND name = 'MetodoPagoId')
    ALTER TABLE [dbo].[Facturas] ADD MetodoPagoId UNIQUEIDENTIFIER NULL
        CONSTRAINT FK_Facturas_MetodosPago FOREIGN KEY REFERENCES [dbo].[MetodosPago](Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Facturas') AND name = 'PaymentReference')
    ALTER TABLE [dbo].[Facturas] ADD PaymentReference NVARCHAR(100) NULL;
GO

-- 2. Actualizar sp_ProcesarVenta para guardar MetodoPagoId y PaymentReference
CREATE OR ALTER PROCEDURE [dbo].[sp_ProcesarVenta]
    @NombreCliente    NVARCHAR(150),
    @UsuarioId        UNIQUEIDENTIFIER = NULL,
    @TotalAmount      DECIMAL(18,2),
    @DetailsJson      NVARCHAR(MAX),
    @TipoOrden        NVARCHAR(50),
    @MetodoPago       NVARCHAR(50)     = NULL,
    @MontoRecibido    DECIMAL(18,2),
    @ClienteId        UNIQUEIDENTIFIER = NULL,
    @MontoDescuento   DECIMAL(18,2)    = 0,
    @Nota             NVARCHAR(500)    = NULL,
    @SubscriptionId   UNIQUEIDENTIFIER = NULL,
    @ServiceOrderId   UNIQUEIDENTIFIER = NULL,
    @MetodoPagoId     UNIQUEIDENTIFIER = NULL,
    @PaymentReference NVARCHAR(100)    = NULL,
    @FacturaId        UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @FacturaId = NEWID();

    DECLARE @NumeroOrden     NVARCHAR(50) = 'ORD-' + RIGHT(CAST(@FacturaId AS VARCHAR(36)), 8);
    DECLARE @PublicLinkToken NVARCHAR(50) = REPLACE(CAST(NEWID() AS VARCHAR(36)), '-', '');

    INSERT INTO [dbo].[Facturas] (
        Id, NumeroOrden, CustomerName, CustomerId, UserId, OrderType, PaymentMethod,
        AmountReceived, DiscountAmount, TotalAmount, Status, SaleDate, Nota,
        PublicLinkToken, SubscriptionId, ServiceOrderId, MetodoPagoId, PaymentReference
    )
    VALUES (
        @FacturaId, @NumeroOrden, @NombreCliente, @ClienteId, @UsuarioId, @TipoOrden, @MetodoPago,
        @MontoRecibido, @MontoDescuento, @TotalAmount, 'Pagada', GETDATE(), @Nota,
        @PublicLinkToken, @SubscriptionId, @ServiceOrderId, @MetodoPagoId, @PaymentReference
    );

    INSERT INTO [dbo].[InvoiceDetails] (Id, InvoiceId, TipoServicioId, Concept, Quantity, UnitPrice, DiscountAmount, SubTotal, Nota)
    SELECT NEWID(), @FacturaId,
           CASE WHEN JSON_VALUE(d.value, '$.TipoServicioId') = '' THEN NULL
                ELSE TRY_CAST(JSON_VALUE(d.value, '$.TipoServicioId') AS UNIQUEIDENTIFIER) END,
           JSON_VALUE(d.value, '$.Concepto'),
           CAST(JSON_VALUE(d.value, '$.Cantidad')       AS INT),
           CAST(JSON_VALUE(d.value, '$.PrecioUnitario') AS DECIMAL(18,2)),
           CAST(JSON_VALUE(d.value, '$.Descuento')      AS DECIMAL(18,2)),
           CAST(JSON_VALUE(d.value, '$.SubTotal')       AS DECIMAL(18,2)),
           JSON_VALUE(d.value, '$.Nota')
    FROM OPENJSON(@DetailsJson) d;
END;
GO

-- 3. sp_ObtenerFacturaPorId con JOIN a MetodosPago
CREATE OR ALTER PROCEDURE [dbo].[sp_ObtenerFacturaPorId]
    @FacturaId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Resultado 1: cabecera de factura
    SELECT
        f.Id,
        f.NumeroOrden,
        f.CustomerName,
        f.CustomerId,
        f.UserId,
        f.OrderType,
        f.PaymentMethod,
        f.AmountReceived,
        f.DiscountAmount,
        f.TotalAmount,
        f.Status,
        f.SaleDate,
        f.Nota,
        f.PublicLinkToken,
        f.SubscriptionId,
        f.ServiceOrderId,
        f.MetodoPagoId,
        f.PaymentReference,

        -- Datos bancarios del método de pago
        mp.Banco            AS MetodoPagoBanco,
        mp.TipoCuenta       AS MetodoPagoTipoCuenta,
        mp.NumeroCuenta     AS MetodoPagoNumeroCuenta,
        mp.Titular          AS MetodoPagoTitular

    FROM [dbo].[Facturas] f
    LEFT JOIN [dbo].[MetodosPago] mp ON mp.Id = f.MetodoPagoId
    WHERE f.Id = @FacturaId;

    -- Resultado 2: líneas de detalle
    SELECT
        d.Id,
        d.InvoiceId,
        d.TipoServicioId,
        d.Concept,
        d.Quantity,
        d.UnitPrice,
        d.DiscountAmount,
        d.SubTotal,
        d.Nota
    FROM [dbo].[InvoiceDetails] d
    WHERE d.InvoiceId = @FacturaId;
END
GO

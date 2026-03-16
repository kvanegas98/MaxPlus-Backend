-- ============================================================
-- Agrega columna ServiceOrderId a Facturas y actualiza sp_ProcesarVenta
-- ============================================================

ALTER TABLE [dbo].[Facturas]
ADD [ServiceOrderId] UNIQUEIDENTIFIER NULL
    CONSTRAINT FK_Facturas_ServiceOrders FOREIGN KEY REFERENCES [dbo].[ServiceOrders]([Id]);
GO

-- ============================================================
-- sp_ProcesarVenta  (actualizado con @ServiceOrderId)
-- ============================================================
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
        PublicLinkToken, SubscriptionId, ServiceOrderId
    )
    VALUES (
        @FacturaId, @NumeroOrden, @NombreCliente, @ClienteId, @UsuarioId, @TipoOrden, @MetodoPago,
        @MontoRecibido, @MontoDescuento, @TotalAmount, 'Pagada', GETDATE(), @Nota,
        @PublicLinkToken, @SubscriptionId, @ServiceOrderId
    );

    INSERT INTO [dbo].[InvoiceDetails] (Id, InvoiceId, TipoServicioId, Concept, Quantity, UnitPrice, DiscountAmount, SubTotal, Nota)
    SELECT NEWID(), @FacturaId,
           CASE WHEN JSON_VALUE(d.value, '$.TipoServicioId') = '' THEN NULL ELSE TRY_CAST(JSON_VALUE(d.value, '$.TipoServicioId') AS UNIQUEIDENTIFIER) END,
           JSON_VALUE(d.value, '$.Concepto'),
           CAST(JSON_VALUE(d.value, '$.Cantidad')       AS INT),
           CAST(JSON_VALUE(d.value, '$.PrecioUnitario') AS DECIMAL(18,2)),
           CAST(JSON_VALUE(d.value, '$.Descuento')      AS DECIMAL(18,2)),
           CAST(JSON_VALUE(d.value, '$.SubTotal')       AS DECIMAL(18,2)),
           JSON_VALUE(d.value, '$.Nota')
    FROM OPENJSON(@DetailsJson) d;
END;
GO

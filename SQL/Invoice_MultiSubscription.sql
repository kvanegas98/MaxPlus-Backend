-- ============================================================
-- Factura directa con múltiples suscripciones
-- SEGURO: no modifica estructura de tablas existentes
-- Fecha: 2026-03-17
-- ============================================================

-- 1. Actualizar sp_Subscriptions_ObtenerSinAsignar — filtro por TipoServicioId
CREATE OR ALTER PROCEDURE [dbo].[sp_Subscriptions_ObtenerSinAsignar]
    @TipoServicioId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        s.Id,
        s.CustomerId,
        s.IptvAccountId,
        s.TipoServicioId,
        s.SubscriptionType,
        s.PlatformUrl,
        s.AccessUser,
        s.AccessPassword,
        s.PinCode,
        s.ProfileUser,
        s.ProfilePin,
        s.StartDate,
        s.ExpirationDate,
        s.Status,
        s.CreatedAt,
        s.UpdatedAt,
        t.Nombre AS ServiceName,
        t.Plataforma
    FROM [dbo].[CustomerSubscriptions] s
    LEFT JOIN [dbo].[TiposServicio] t ON t.Id = s.TipoServicioId
    WHERE s.Status = 'Unassigned'
      AND (@TipoServicioId IS NULL OR s.TipoServicioId = @TipoServicioId)
    ORDER BY s.CreatedAt DESC;
END
GO

-- 2. Actualizar sp_ProcesarVenta — activa suscripciones referenciadas en los detalles
CREATE OR ALTER PROCEDURE [dbo].[sp_ProcesarVenta]
    @NombreCliente   NVARCHAR(150),
    @UsuarioId       UNIQUEIDENTIFIER,
    @TotalAmount     DECIMAL(18,2),
    @DetailsJson     NVARCHAR(MAX),
    @TipoOrden       NVARCHAR(50),
    @MetodoPago      NVARCHAR(50),
    @MontoRecibido   DECIMAL(18,2),
    @ClienteId       UNIQUEIDENTIFIER = NULL,
    @MontoDescuento  DECIMAL(18,2)    = 0,
    @Nota            NVARCHAR(500)    = NULL,
    @SubscriptionId  UNIQUEIDENTIFIER = NULL,
    @ServiceOrderId  UNIQUEIDENTIFIER = NULL,
    @MetodoPagoId    UNIQUEIDENTIFIER = NULL,
    @PaymentReference NVARCHAR(100)   = NULL,
    @FacturaId       UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @FacturaId = NEWID();

    DECLARE @NumeroOrden    NVARCHAR(50) = 'ORD-' + UPPER(LEFT(REPLACE(CAST(@FacturaId AS NVARCHAR(36)), '-', ''), 8));
    DECLARE @PublicLinkToken NVARCHAR(50) = REPLACE(CAST(NEWID() AS VARCHAR(36)), '-', '');

    -- Insertar factura
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

    -- Insertar detalles de factura
    INSERT INTO [dbo].[InvoiceDetails] (Id, InvoiceId, TipoServicioId, Concept, Quantity, UnitPrice, DiscountAmount, SubTotal, Nota)
    SELECT
        NEWID(),
        @FacturaId,
        CASE WHEN JSON_VALUE(d.value, '$.TipoServicioId') = '' OR JSON_VALUE(d.value, '$.TipoServicioId') IS NULL
             THEN NULL
             ELSE TRY_CAST(JSON_VALUE(d.value, '$.TipoServicioId') AS UNIQUEIDENTIFIER)
        END,
        JSON_VALUE(d.value, '$.Concepto'),
        CAST(JSON_VALUE(d.value, '$.Cantidad') AS INT),
        CAST(JSON_VALUE(d.value, '$.PrecioUnitario') AS DECIMAL(18,2)),
        CAST(JSON_VALUE(d.value, '$.Descuento') AS DECIMAL(18,2)),
        CAST(JSON_VALUE(d.value, '$.SubTotal') AS DECIMAL(18,2)),
        JSON_VALUE(d.value, '$.Nota')
    FROM OPENJSON(@DetailsJson) d;

    -- Activar suscripciones Unassigned referenciadas en los detalles
    -- Solo si el ClienteId está presente
    -- Si el detalle trae DurationMonths, calcula ExpirationDate desde TiposServicio
    IF @ClienteId IS NOT NULL
    BEGIN
        UPDATE cs
        SET cs.CustomerId     = @ClienteId,
            cs.Status         = 'Active',
            cs.ExpirationDate = CASE
                WHEN TRY_CAST(JSON_VALUE(d.value, '$.DurationMonths') AS INT) IS NOT NULL
                     AND TRY_CAST(JSON_VALUE(d.value, '$.DurationMonths') AS INT) > 0
                     AND t.DurationDays IS NOT NULL
                THEN DATEADD(DAY,
                         t.DurationDays * TRY_CAST(JSON_VALUE(d.value, '$.DurationMonths') AS INT),
                         GETDATE())
                ELSE cs.ExpirationDate
            END,
            cs.UpdatedAt      = GETDATE()
        FROM [dbo].[CustomerSubscriptions] cs
        INNER JOIN OPENJSON(@DetailsJson) d
            ON cs.Id = TRY_CAST(JSON_VALUE(d.value, '$.SubscriptionId') AS UNIQUEIDENTIFIER)
        LEFT JOIN [dbo].[TiposServicio] t ON t.Id = cs.TipoServicioId
        WHERE cs.Status = 'Unassigned'
          AND JSON_VALUE(d.value, '$.SubscriptionId') IS NOT NULL
          AND JSON_VALUE(d.value, '$.SubscriptionId') != '';
    END
END
GO

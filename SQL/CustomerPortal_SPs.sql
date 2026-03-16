-- ============================================================
-- Portal del cliente: SPs de consulta por teléfono / clienteId
-- Fecha: 2026-03-13
-- ============================================================

-- ============================================================
-- SP: Facturas de un cliente (sin detalle — solo cabecera)
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Facturas_PorCliente]
    @ClienteId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

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
        f.PaymentReference
    FROM [dbo].[Facturas] f
    WHERE f.CustomerId = @ClienteId
      AND f.Status     = 'Pagada'
    ORDER BY f.SaleDate DESC;
END
GO

-- ============================================================
-- SP: Demos solicitadas por número de teléfono
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_DemoRequests_PorTelefono]
    @Phone NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        dr.Id,
        dr.CustomerName,
        dr.CustomerPhone,
        dr.CustomerEmail,
        dr.CustomerId,
        dr.TipoServicioId,
        ts.Nombre   AS ServiceName,
        dr.Status,
        dr.DemoUrl,
        dr.ExpiresAt,
        dr.RejectionReason,
        dr.ApprovedAt,
        dr.CreatedAt
    FROM [dbo].[DemoRequests] dr
    LEFT JOIN [dbo].[TiposServicio] ts ON ts.Id = dr.TipoServicioId
    WHERE dr.CustomerPhone = @Phone
    ORDER BY dr.CreatedAt DESC;
END
GO

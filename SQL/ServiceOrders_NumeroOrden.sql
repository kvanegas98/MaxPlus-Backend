-- ============================================================
-- Migración: Agregar NumeroOrden consecutivo a ServiceOrders
-- Fecha: 2026-03-13
-- Formato: WEB-000001, WEB-000002, ...
-- ============================================================

-- 1. Agregar columna si no existe
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('ServiceOrders') AND name = 'NumeroOrden'
)
BEGIN
    ALTER TABLE [dbo].[ServiceOrders]
    ADD NumeroOrden NVARCHAR(20) NULL;
END
GO

-- 2. Backfill para órdenes existentes (orden por fecha de creación)
WITH Numeradas AS (
    SELECT Id,
           ROW_NUMBER() OVER (ORDER BY CreatedAt ASC) AS Rn
    FROM [dbo].[ServiceOrders]
    WHERE NumeroOrden IS NULL
)
UPDATE so
SET so.NumeroOrden = 'WEB-' + RIGHT('000000' + CAST(n.Rn AS NVARCHAR(10)), 6)
FROM [dbo].[ServiceOrders] so
INNER JOIN Numeradas n ON n.Id = so.Id;
GO

-- ============================================================
-- SP: Crear orden — genera NumeroOrden consecutivo
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ServiceOrders_Crear]
    @CustomerName   NVARCHAR(150),
    @CustomerPhone  NVARCHAR(20)     = NULL,
    @CustomerEmail  NVARCHAR(100)    = NULL,
    @TipoServicioId UNIQUEIDENTIFIER = NULL,
    @Notes          NVARCHAR(500)    = NULL,
    @IpAddress      NVARCHAR(50)     = NULL,
    @Id             UNIQUEIDENTIFIER OUTPUT,
    @NumeroOrden    NVARCHAR(20)     OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Id = NEWID();

    -- Generar consecutivo basado en el máximo existente
    DECLARE @Consecutivo INT;
    SELECT @Consecutivo = ISNULL(
        MAX(CAST(SUBSTRING(NumeroOrden, 5, 10) AS INT)), 0
    ) + 1
    FROM [dbo].[ServiceOrders]
    WHERE NumeroOrden LIKE 'WEB-%';

    SET @NumeroOrden = 'WEB-' + RIGHT('000000' + CAST(@Consecutivo AS NVARCHAR(10)), 6);

    INSERT INTO [dbo].[ServiceOrders]
        (Id, NumeroOrden, CustomerName, CustomerPhone, CustomerEmail,
         TipoServicioId, Notes, IpAddress, Status, CreatedAt)
    VALUES
        (@Id, @NumeroOrden, @CustomerName, @CustomerPhone, @CustomerEmail,
         @TipoServicioId, @Notes, @IpAddress, 'Pending', GETDATE());
END
GO

-- ============================================================
-- SP: Obtener por Id — incluye NumeroOrden
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ServiceOrders_ObtenerPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        so.Id,
        so.NumeroOrden,
        so.CustomerName,
        so.CustomerPhone,
        so.CustomerEmail,
        so.TipoServicioId,
        ts.Nombre   AS ServiceName,
        so.Notes,
        so.Status,
        so.ApprovedBy,
        so.ApprovedAt,
        so.SubscriptionId,
        so.RejectionReason,
        so.IpAddress,
        so.CreatedAt
    FROM [dbo].[ServiceOrders] so
    LEFT JOIN [dbo].[TiposServicio] ts ON ts.Id = so.TipoServicioId
    WHERE so.Id = @Id;
END
GO

-- ============================================================
-- SP: Obtener todos — incluye NumeroOrden
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ServiceOrders_ObtenerTodos]
    @Status NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        so.Id,
        so.NumeroOrden,
        so.CustomerName,
        so.CustomerPhone,
        so.CustomerEmail,
        so.TipoServicioId,
        ts.Nombre   AS ServiceName,
        so.Notes,
        so.Status,
        so.ApprovedBy,
        so.ApprovedAt,
        so.SubscriptionId,
        so.RejectionReason,
        so.IpAddress,
        so.CreatedAt
    FROM [dbo].[ServiceOrders] so
    LEFT JOIN [dbo].[TiposServicio] ts ON ts.Id = so.TipoServicioId
    WHERE (@Status IS NULL OR so.Status = @Status)
    ORDER BY so.CreatedAt DESC;
END
GO

-- ============================================================
-- Dashboard: Resumen operativo + financiero
-- Fecha: 2026-03-13
-- ============================================================

-- ============================================================
-- SP: Suscripciones por vencer
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Dashboard_PorVencer]
    @DaysAhead INT = 30
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        cs.Id,
        cs.CustomerId,
        c.Nombre          AS CustomerName,
        c.Telefono        AS CustomerPhone,
        ts.Nombre         AS ServiceName,
        cs.Status,
        cs.ExpirationDate,
        DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(cs.ExpirationDate AS DATE)) AS DaysRemaining,
        cs.AccessUser
    FROM [dbo].[CustomerSubscriptions] cs
    LEFT JOIN [dbo].[Clientes]      c  ON c.Id  = cs.CustomerId
    LEFT JOIN [dbo].[TiposServicio] ts ON ts.Id = cs.TipoServicioId
    WHERE cs.Status = 'Active'
      AND cs.ExpirationDate >= CAST(GETDATE() AS DATE)
      AND cs.ExpirationDate <= DATEADD(DAY, @DaysAhead, CAST(GETDATE() AS DATE))
    ORDER BY cs.ExpirationDate ASC;
END
GO

-- ============================================================
-- SP: Resumen financiero (ingresos, costos, ganancia)
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Dashboard_ResumenFinanciero]
    @FechaDesde DATE = NULL,
    @FechaHasta DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Por defecto: mes actual
    IF @FechaDesde IS NULL SET @FechaDesde = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    IF @FechaHasta IS NULL SET @FechaHasta = EOMONTH(GETDATE());

    -- Ingresos: facturas pagadas en el período
    DECLARE @Ingresos      DECIMAL(18,2);
    DECLARE @TotalFacturas INT;

    SELECT
        @Ingresos      = ISNULL(SUM(TotalAmount), 0),
        @TotalFacturas = COUNT(*)
    FROM [dbo].[Facturas]
    WHERE Status    = 'Pagada'
      AND CAST(SaleDate AS DATE) BETWEEN @FechaDesde AND @FechaHasta;

    -- Costos: cuentas IPTV compradas en el período (PurchasePrice)
    DECLARE @Costos      DECIMAL(18,2);
    DECLARE @CuentasCompradas INT;

    SELECT
        @Costos          = ISNULL(SUM(PurchasePrice), 0),
        @CuentasCompradas = COUNT(*)
    FROM [dbo].[IptvAccounts]
    WHERE PurchasePrice IS NOT NULL
      AND CAST(CreatedAt AS DATE) BETWEEN @FechaDesde AND @FechaHasta;

    -- Suscripciones activas totales
    DECLARE @ActiveSubscriptions  INT;
    DECLARE @ExpiredSubscriptions INT;

    SELECT @ActiveSubscriptions  = COUNT(*) FROM [dbo].[CustomerSubscriptions] WHERE Status = 'Active';
    SELECT @ExpiredSubscriptions = COUNT(*) FROM [dbo].[CustomerSubscriptions] WHERE Status = 'Expired';

    -- Vencimientos próximos (7 y 30 días)
    DECLARE @ExpiringIn7  INT;
    DECLARE @ExpiringIn30 INT;

    SELECT @ExpiringIn7 = COUNT(*)
    FROM [dbo].[CustomerSubscriptions]
    WHERE Status = 'Active'
      AND ExpirationDate >= CAST(GETDATE() AS DATE)
      AND ExpirationDate <= DATEADD(DAY, 7, CAST(GETDATE() AS DATE));

    SELECT @ExpiringIn30 = COUNT(*)
    FROM [dbo].[CustomerSubscriptions]
    WHERE Status = 'Active'
      AND ExpirationDate >= CAST(GETDATE() AS DATE)
      AND ExpirationDate <= DATEADD(DAY, 30, CAST(GETDATE() AS DATE));

    -- Ingresos por mes (últimos 6 meses) para gráfica
    SELECT
        FORMAT(SaleDate, 'yyyy-MM') AS Mes,
        SUM(TotalAmount)            AS Ingresos,
        COUNT(*)                    AS Facturas
    INTO #MensualIngresos
    FROM [dbo].[Facturas]
    WHERE Status = 'Pagada'
      AND SaleDate >= DATEADD(MONTH, -5, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
    GROUP BY FORMAT(SaleDate, 'yyyy-MM')
    ORDER BY Mes;

    -- Resultado: resumen general
    SELECT
        @Ingresos             AS Ingresos,
        @Costos               AS Costos,
        (@Ingresos - @Costos) AS Ganancia,
        @TotalFacturas        AS TotalFacturas,
        @CuentasCompradas     AS CuentasCompradas,
        @ActiveSubscriptions  AS ActiveSubscriptions,
        @ExpiredSubscriptions AS ExpiredSubscriptions,
        @ExpiringIn7          AS ExpiringIn7Days,
        @ExpiringIn30         AS ExpiringIn30Days,
        @FechaDesde           AS FechaDesde,
        @FechaHasta           AS FechaHasta;

    -- Resultado: detalle mensual
    SELECT * FROM #MensualIngresos ORDER BY Mes;

    DROP TABLE #MensualIngresos;
END
GO

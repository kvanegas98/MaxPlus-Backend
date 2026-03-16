-- ============================================================
-- Migración: Agregar AccessEmail a IptvAccounts
-- Fecha: 2026-03-13
-- Netflix/Streaming: la cuenta tiene correo + contraseña compartidos.
-- IPTV/FlujoTV: este campo queda NULL.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('IptvAccounts') AND name = 'AccessEmail'
)
BEGIN
    ALTER TABLE [dbo].[IptvAccounts]
    ADD AccessEmail NVARCHAR(150) NULL;
END
GO

-- ============================================================
-- SP: Crear cuenta — incluye AccessEmail
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_Crear]
    @AccessUser     NVARCHAR(100),
    @AccessPassword NVARCHAR(100),
    @AccessEmail    NVARCHAR(150)    = NULL,
    @PlatformUrl    NVARCHAR(255)    = NULL,
    @PinCode        NVARCHAR(20)     = NULL,
    @TipoServicioId UNIQUEIDENTIFIER = NULL,
    @MaxSlots       INT              = 1,
    @PurchasePrice  DECIMAL(18,2)    = 0,
    @ExpirationDate DATETIME2        = NULL,
    @Notes          NVARCHAR(500)    = NULL,
    @Id             UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();

    INSERT INTO [dbo].[IptvAccounts]
        (Id, AccessUser, AccessPassword, AccessEmail, PlatformUrl, PinCode,
         TipoServicioId, MaxSlots, PurchasePrice, ExpirationDate, Notes, IsActive, CreatedAt)
    VALUES
        (@Id, @AccessUser, @AccessPassword, @AccessEmail, @PlatformUrl, @PinCode,
         @TipoServicioId, @MaxSlots, @PurchasePrice, @ExpirationDate, @Notes, 1, GETDATE());
END
GO

-- ============================================================
-- SP: Actualizar cuenta — incluye AccessEmail
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_Actualizar]
    @Id             UNIQUEIDENTIFIER,
    @AccessUser     NVARCHAR(100),
    @AccessPassword NVARCHAR(100),
    @AccessEmail    NVARCHAR(150)    = NULL,
    @PlatformUrl    NVARCHAR(255)    = NULL,
    @PinCode        NVARCHAR(20)     = NULL,
    @TipoServicioId UNIQUEIDENTIFIER = NULL,
    @MaxSlots       INT,
    @PurchasePrice  DECIMAL(18,2),
    @ExpirationDate DATETIME2        = NULL,
    @Notes          NVARCHAR(500)    = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[IptvAccounts]
    SET AccessUser     = @AccessUser,
        AccessPassword = @AccessPassword,
        AccessEmail    = @AccessEmail,
        PlatformUrl    = @PlatformUrl,
        PinCode        = @PinCode,
        TipoServicioId = @TipoServicioId,
        MaxSlots       = @MaxSlots,
        PurchasePrice  = @PurchasePrice,
        ExpirationDate = @ExpirationDate,
        Notes          = @Notes,
        UpdatedAt      = GETDATE()
    WHERE Id = @Id;
END
GO

-- ============================================================
-- SP: Obtener todas las cuentas — incluye AccessEmail
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_ObtenerTodos]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.Id,
        a.AccessUser,
        a.AccessPassword,
        a.AccessEmail,
        a.PlatformUrl,
        a.PinCode,
        a.TipoServicioId,
        ts.Nombre AS ServiceName,
        a.MaxSlots,
        a.PurchasePrice,
        a.ExpirationDate,
        CASE
            WHEN a.ExpirationDate IS NOT NULL
            THEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(a.ExpirationDate AS DATE))
            ELSE NULL
        END AS DaysRemaining,
        a.Notes,
        a.IsActive,
        a.CreatedAt,
        a.UpdatedAt,
        (
            SELECT COUNT(*)
            FROM [dbo].[CustomerSubscriptions] cs
            WHERE cs.IptvAccountId = a.Id
              AND cs.Status NOT IN ('Cancelled', 'Renewed')
        ) AS UsedSlots
    FROM [dbo].[IptvAccounts] a
    LEFT JOIN [dbo].[TiposServicio] ts ON ts.Id = a.TipoServicioId
    WHERE a.IsActive = 1
    ORDER BY a.CreatedAt DESC;
END
GO

-- ============================================================
-- SP: Obtener por Id — incluye AccessEmail
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_ObtenerPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.Id,
        a.AccessUser,
        a.AccessPassword,
        a.AccessEmail,
        a.PlatformUrl,
        a.PinCode,
        a.TipoServicioId,
        ts.Nombre AS ServiceName,
        a.MaxSlots,
        a.PurchasePrice,
        a.ExpirationDate,
        CASE
            WHEN a.ExpirationDate IS NOT NULL
            THEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(a.ExpirationDate AS DATE))
            ELSE NULL
        END AS DaysRemaining,
        a.Notes,
        a.IsActive,
        a.CreatedAt,
        a.UpdatedAt
    FROM [dbo].[IptvAccounts] a
    LEFT JOIN [dbo].[TiposServicio] ts ON ts.Id = a.TipoServicioId
    WHERE a.Id = @Id;
END
GO

-- ============================================================
-- SP: Obtener disponibles por tipo de servicio — incluye AccessEmail
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_PorTipoServicio]
    @TipoServicioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.Id,
        a.AccessUser,
        a.AccessPassword,
        a.AccessEmail,
        a.PlatformUrl,
        a.PinCode,
        a.TipoServicioId,
        ts.Nombre AS ServiceName,
        a.MaxSlots,
        a.PurchasePrice,
        a.ExpirationDate,
        CASE
            WHEN a.ExpirationDate IS NOT NULL
            THEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(a.ExpirationDate AS DATE))
            ELSE NULL
        END AS DaysRemaining,
        a.Notes,
        a.IsActive,
        a.CreatedAt,
        a.UpdatedAt,
        (
            SELECT COUNT(*)
            FROM [dbo].[CustomerSubscriptions] cs
            WHERE cs.IptvAccountId = a.Id
              AND cs.Status NOT IN ('Cancelled', 'Renewed')
        ) AS UsedSlots
    FROM [dbo].[IptvAccounts] a
    LEFT JOIN [dbo].[TiposServicio] ts ON ts.Id = a.TipoServicioId
    WHERE a.IsActive = 1
      AND a.TipoServicioId = @TipoServicioId
      AND (
            SELECT COUNT(*)
            FROM [dbo].[CustomerSubscriptions] cs
            WHERE cs.IptvAccountId = a.Id
              AND cs.Status NOT IN ('Cancelled', 'Renewed')
          ) < a.MaxSlots
    ORDER BY a.ExpirationDate ASC;
END
GO

-- ============================================================
-- Fix: quitar TipoServicioId + agregar ExpirationDate a IptvAccounts
-- ============================================================

-- Agregar ExpirationDate si no existe
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('IptvAccounts') AND name = 'ExpirationDate')
BEGIN
    ALTER TABLE [dbo].[IptvAccounts]
    ADD ExpirationDate DATETIME2 NULL;
END
GO

-- Quitar FK y columna TipoServicioId si existe
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('IptvAccounts') AND name = 'TipoServicioId')
BEGIN
    -- Quitar FK primero
    DECLARE @fk NVARCHAR(200) = (
        SELECT fk.name
        FROM sys.foreign_keys fk
        JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
        WHERE fk.parent_object_id = OBJECT_ID('IptvAccounts')
          AND c.name = 'TipoServicioId'
    );
    IF @fk IS NOT NULL
        EXEC('ALTER TABLE IptvAccounts DROP CONSTRAINT ' + @fk);

    ALTER TABLE [dbo].[IptvAccounts] DROP COLUMN TipoServicioId;
END
GO

-- ── SPs actualizados sin TipoServicioId ─────────────────────

CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_Crear]
    @AccessUser     NVARCHAR(100),
    @AccessPassword NVARCHAR(100),
    @PlatformUrl    NVARCHAR(255)    = NULL,
    @PinCode        NVARCHAR(20)     = NULL,
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
        (Id, AccessUser, AccessPassword, PlatformUrl, PinCode,
         MaxSlots, PurchasePrice, ExpirationDate, Notes, IsActive, CreatedAt)
    VALUES
        (@Id, @AccessUser, @AccessPassword, @PlatformUrl, @PinCode,
         @MaxSlots, @PurchasePrice, @ExpirationDate, @Notes, 1, GETDATE());
END
GO

-- ────────────────────────────────────────────────────────────

CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_ObtenerTodos]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.Id,
        a.AccessUser,
        a.AccessPassword,
        a.PlatformUrl,
        a.PinCode,
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
    WHERE a.IsActive = 1
    ORDER BY a.CreatedAt DESC;
END
GO

-- ────────────────────────────────────────────────────────────

CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_ObtenerPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.Id,
        a.AccessUser,
        a.AccessPassword,
        a.PlatformUrl,
        a.PinCode,
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
    WHERE a.Id = @Id;
END
GO

-- ────────────────────────────────────────────────────────────

CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_Actualizar]
    @Id             UNIQUEIDENTIFIER,
    @AccessUser     NVARCHAR(100),
    @AccessPassword NVARCHAR(100),
    @PlatformUrl    NVARCHAR(255)    = NULL,
    @PinCode        NVARCHAR(20)     = NULL,
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
        PlatformUrl    = @PlatformUrl,
        PinCode        = @PinCode,
        MaxSlots       = @MaxSlots,
        PurchasePrice  = @PurchasePrice,
        ExpirationDate = @ExpirationDate,
        Notes          = @Notes,
        UpdatedAt      = GETDATE()
    WHERE Id = @Id;
END
GO

-- ────────────────────────────────────────────────────────────

CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_ObtenerConClientes]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.Id              AS AccountId,
        a.AccessUser,
        a.AccessPassword,
        a.PlatformUrl,
        a.PinCode,
        a.MaxSlots,
        a.PurchasePrice,
        a.ExpirationDate  AS AccountExpirationDate,
        CASE
            WHEN a.ExpirationDate IS NOT NULL
            THEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(a.ExpirationDate AS DATE))
            ELSE NULL
        END               AS AccountDaysRemaining,
        a.Notes,
        a.IsActive,
        a.CreatedAt,

        cs.Id             AS SubscriptionId,
        cs.CustomerId,
        c.Nombre          AS CustomerName,
        c.Telefono        AS CustomerPhone,
        c.Email           AS CustomerEmail,
        cs.StartDate,
        cs.ExpirationDate,
        cs.Status         AS SlotStatus,
        CASE
            WHEN cs.ExpirationDate IS NOT NULL
            THEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(cs.ExpirationDate AS DATE))
            ELSE NULL
        END               AS DaysRemaining

    FROM [dbo].[IptvAccounts] a
    LEFT JOIN [dbo].[CustomerSubscriptions] cs
           ON cs.IptvAccountId = a.Id
          AND cs.Status NOT IN ('Cancelled', 'Renewed')
    LEFT JOIN [dbo].[Clientes] c
           ON c.Id = cs.CustomerId

    WHERE a.IsActive = 1
    ORDER BY a.CreatedAt DESC, cs.ExpirationDate;
END
GO

-- ────────────────────────────────────────────────────────────
-- Stats para el dashboard
-- ────────────────────────────────────────────────────────────

CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_Stats]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(DISTINCT a.Id) AS TotalAccounts,
        ISNULL(SUM(a.MaxSlots), 0) AS TotalSlots,
        ISNULL((
            SELECT COUNT(*)
            FROM [dbo].[CustomerSubscriptions] cs
            WHERE cs.IptvAccountId IS NOT NULL
              AND cs.Status NOT IN ('Cancelled', 'Renewed')
        ), 0) AS UsedSlots
    FROM [dbo].[IptvAccounts] a
    WHERE a.IsActive = 1;
END
GO

-- ────────────────────────────────────────────────────────────
-- Asignar cliente: ahora recibe TipoServicioId para guardarlo en el slot
-- ────────────────────────────────────────────────────────────

CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_AsignarCliente]
    @IptvAccountId  UNIQUEIDENTIFIER,
    @CustomerId     UNIQUEIDENTIFIER,
    @ExpirationDate DATETIME2,
    @TipoServicioId UNIQUEIDENTIFIER = NULL,
    @NewSubId       UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Verificar disponibilidad de slots
    DECLARE @UsedSlots INT = (
        SELECT COUNT(*)
        FROM [dbo].[CustomerSubscriptions]
        WHERE IptvAccountId = @IptvAccountId
          AND Status NOT IN ('Cancelled', 'Renewed')
    );

    DECLARE @MaxSlots INT = (
        SELECT MaxSlots FROM [dbo].[IptvAccounts] WHERE Id = @IptvAccountId
    );

    IF @MaxSlots IS NULL
        THROW 50011, 'La cuenta IPTV no existe.', 1;

    IF @UsedSlots >= @MaxSlots
        THROW 50010, 'No hay slots disponibles en esta cuenta IPTV.', 1;

    SET @NewSubId = NEWID();

    INSERT INTO [dbo].[CustomerSubscriptions]
        (Id, IptvAccountId, CustomerId, TipoServicioId, StartDate, ExpirationDate, Status, CreatedAt)
    VALUES
        (@NewSubId, @IptvAccountId, @CustomerId, @TipoServicioId, GETDATE(), @ExpirationDate, 'Active', GETDATE());
END
GO

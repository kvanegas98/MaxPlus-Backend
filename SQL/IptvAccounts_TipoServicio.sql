-- ============================================================
-- Migración: Agregar TipoServicioId a IptvAccounts
-- Fecha: 2026-03-12
-- Asocia cada cuenta con un tipo de servicio para que al asignar
-- clientes se hereden automáticamente precio, duración y plataforma.
-- ============================================================

-- 1. Agregar columna (si no existe)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('IptvAccounts') AND name = 'TipoServicioId')
BEGIN
    ALTER TABLE [dbo].[IptvAccounts]
    ADD TipoServicioId UNIQUEIDENTIFIER NULL
        REFERENCES [dbo].[TiposServicio](Id);
END
GO

-- ============================================================
-- SP: Crear cuenta con TipoServicioId
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_Crear]
    @AccessUser     NVARCHAR(100),
    @AccessPassword NVARCHAR(100),
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
        (Id, AccessUser, AccessPassword, PlatformUrl, PinCode,
         TipoServicioId, MaxSlots, PurchasePrice, ExpirationDate, Notes, IsActive, CreatedAt)
    VALUES
        (@Id, @AccessUser, @AccessPassword, @PlatformUrl, @PinCode,
         @TipoServicioId, @MaxSlots, @PurchasePrice, @ExpirationDate, @Notes, 1, GETDATE());
END
GO

-- ============================================================
-- SP: Actualizar cuenta con TipoServicioId
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_Actualizar]
    @Id             UNIQUEIDENTIFIER,
    @AccessUser     NVARCHAR(100),
    @AccessPassword NVARCHAR(100),
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
-- SP: Obtener todas las cuentas (incluye ServiceName)
-- ============================================================
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
-- SP: Obtener por Id (incluye ServiceName)
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
-- SP: Obtener con clientes (incluye TipoServicioId y ServiceName en cuenta)
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_ObtenerConClientes]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        -- Datos de la cuenta
        a.Id              AS AccountId,
        a.AccessUser,
        a.AccessPassword,
        a.PlatformUrl,
        a.PinCode,
        a.TipoServicioId,
        ts.Nombre         AS ServiceName,
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

        -- Datos del slot / cliente (NULL si no hay cliente asignado)
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
    LEFT JOIN [dbo].[TiposServicio] ts
           ON ts.Id = a.TipoServicioId
    LEFT JOIN [dbo].[CustomerSubscriptions] cs
           ON cs.IptvAccountId = a.Id
          AND cs.Status NOT IN ('Cancelled', 'Renewed')
    LEFT JOIN [dbo].[Clientes] c
           ON c.Id = cs.CustomerId

    WHERE a.IsActive = 1
    ORDER BY a.CreatedAt DESC, cs.ExpirationDate;
END
GO

-- ============================================================
-- SP: Asignar cliente — hereda TipoServicioId de la cuenta si no se especifica
-- ============================================================
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

    -- Si no se especificó TipoServicioId, heredar del IptvAccount
    IF @TipoServicioId IS NULL
        SELECT @TipoServicioId = TipoServicioId
        FROM [dbo].[IptvAccounts]
        WHERE Id = @IptvAccountId;

    SET @NewSubId = NEWID();

    INSERT INTO [dbo].[CustomerSubscriptions]
        (Id, IptvAccountId, CustomerId, TipoServicioId, StartDate, ExpirationDate, Status, CreatedAt)
    VALUES
        (@NewSubId, @IptvAccountId, @CustomerId, @TipoServicioId, GETDATE(), @ExpirationDate, 'Active', GETDATE());
END
GO

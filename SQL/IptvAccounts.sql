-- ============================================================
-- IptvAccounts: Cuentas IPTV compradas al proveedor
-- Cada cuenta puede tener N clientes (slots)
-- ============================================================

-- 1. Crear tabla IptvAccounts
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IptvAccounts')
BEGIN
    CREATE TABLE [dbo].[IptvAccounts] (
        Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        AccessUser      NVARCHAR(100)    NOT NULL,
        AccessPassword  NVARCHAR(100)    NOT NULL,
        PlatformUrl     NVARCHAR(255)    NULL,
        PinCode         NVARCHAR(20)     NULL,
        TipoServicioId  UNIQUEIDENTIFIER NULL REFERENCES TiposServicio(Id),
        MaxSlots        INT              NOT NULL DEFAULT 1,
        PurchasePrice   DECIMAL(18,2)    NOT NULL DEFAULT 0,
        Notes           NVARCHAR(500)    NULL,
        IsActive        BIT              NOT NULL DEFAULT 1,
        CreatedAt       DATETIME2        NOT NULL DEFAULT GETDATE(),
        UpdatedAt       DATETIME2        NULL
    );
END
GO

-- 2. Agregar IptvAccountId a CustomerSubscriptions (nullable para compatibilidad)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CustomerSubscriptions') AND name = 'IptvAccountId')
BEGIN
    ALTER TABLE [dbo].[CustomerSubscriptions]
    ADD IptvAccountId UNIQUEIDENTIFIER NULL
        REFERENCES IptvAccounts(Id);
END
GO

-- ============================================================
-- SPs de IptvAccounts
-- ============================================================

CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_Crear]
    @AccessUser     NVARCHAR(100),
    @AccessPassword NVARCHAR(100),
    @PlatformUrl    NVARCHAR(255)    = NULL,
    @PinCode        NVARCHAR(20)     = NULL,
    @TipoServicioId UNIQUEIDENTIFIER = NULL,
    @MaxSlots       INT              = 1,
    @PurchasePrice  DECIMAL(18,2)    = 0,
    @Notes          NVARCHAR(500)    = NULL,
    @Id             UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();

    INSERT INTO [dbo].[IptvAccounts]
        (Id, AccessUser, AccessPassword, PlatformUrl, PinCode,
         TipoServicioId, MaxSlots, PurchasePrice, Notes, IsActive, CreatedAt)
    VALUES
        (@Id, @AccessUser, @AccessPassword, @PlatformUrl, @PinCode,
         @TipoServicioId, @MaxSlots, @PurchasePrice, @Notes, 1, GETDATE());
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
        a.TipoServicioId,
        ts.Nombre         AS ServiceName,
        a.MaxSlots,
        a.PurchasePrice,
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
        a.TipoServicioId,
        ts.Nombre AS ServiceName,
        a.MaxSlots,
        a.PurchasePrice,
        a.Notes,
        a.IsActive,
        a.CreatedAt,
        a.UpdatedAt
    FROM [dbo].[IptvAccounts] a
    LEFT JOIN [dbo].[TiposServicio] ts ON ts.Id = a.TipoServicioId
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
    @TipoServicioId UNIQUEIDENTIFIER = NULL,
    @MaxSlots       INT,
    @PurchasePrice  DECIMAL(18,2),
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
        Notes          = @Notes,
        UpdatedAt      = GETDATE()
    WHERE Id = @Id;
END
GO

-- ────────────────────────────────────────────────────────────

CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_Desactivar]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[IptvAccounts]
    SET IsActive  = 0,
        UpdatedAt = GETDATE()
    WHERE Id = @Id;
END
GO

-- ────────────────────────────────────────────────────────────
-- Vista agrupada: cuenta + clientes asignados con días restantes
-- ────────────────────────────────────────────────────────────

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

-- ────────────────────────────────────────────────────────────
-- Asignar un cliente a un slot de la cuenta
-- ────────────────────────────────────────────────────────────

CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_AsignarCliente]
    @IptvAccountId  UNIQUEIDENTIFIER,
    @CustomerId     UNIQUEIDENTIFIER,
    @ExpirationDate DATETIME2,
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

    -- Crear el slot (CustomerSubscription)
    SET @NewSubId = NEWID();

    INSERT INTO [dbo].[CustomerSubscriptions]
        (Id, IptvAccountId, CustomerId, StartDate, ExpirationDate, Status, CreatedAt)
    VALUES
        (@NewSubId, @IptvAccountId, @CustomerId, GETDATE(), @ExpirationDate, 'Active', GETDATE());
END
GO

-- ────────────────────────────────────────────────────────────
-- Actualizar sp_CustomerSubscriptions_Renovar para heredar IptvAccountId
-- ────────────────────────────────────────────────────────────

CREATE OR ALTER PROCEDURE [dbo].[sp_CustomerSubscriptions_Renovar]
    @SubscriptionId UNIQUEIDENTIFIER,
    @NewExpiration  DATETIME2,
    @NewId          UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @CustomerId       UNIQUEIDENTIFIER,
        @TipoServicioId   UNIQUEIDENTIFIER,
        @SubscriptionType NVARCHAR(50),
        @PlatformUrl      NVARCHAR(255),
        @AccessUser       NVARCHAR(100),
        @AccessPassword   NVARCHAR(100),
        @PinCode          NVARCHAR(20),
        @IptvAccountId    UNIQUEIDENTIFIER;

    SELECT
        @CustomerId       = CustomerId,
        @TipoServicioId   = TipoServicioId,
        @SubscriptionType = SubscriptionType,
        @PlatformUrl      = PlatformUrl,
        @AccessUser       = AccessUser,
        @AccessPassword   = AccessPassword,
        @PinCode          = PinCode,
        @IptvAccountId    = IptvAccountId
    FROM [dbo].[CustomerSubscriptions]
    WHERE Id = @SubscriptionId;

    -- Marcar la anterior como Renewed
    UPDATE [dbo].[CustomerSubscriptions]
    SET Status    = 'Renewed',
        UpdatedAt = GETDATE()
    WHERE Id = @SubscriptionId;

    -- Crear nueva suscripcion
    SET @NewId = NEWID();

    INSERT INTO [dbo].[CustomerSubscriptions]
        (Id, IptvAccountId, CustomerId, TipoServicioId, SubscriptionType,
         PlatformUrl, AccessUser, AccessPassword, PinCode,
         StartDate, ExpirationDate, Status, CreatedAt)
    VALUES
        (@NewId, @IptvAccountId, @CustomerId, @TipoServicioId, @SubscriptionType,
         @PlatformUrl, @AccessUser, @AccessPassword, @PinCode,
         GETDATE(), @NewExpiration, 'Active', GETDATE());
END
GO

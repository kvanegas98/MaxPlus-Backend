-- ============================================================
-- Migración: Agregar credenciales de perfil a CustomerSubscriptions
-- Fecha: 2026-03-13
-- Netflix/Streaming: cada slot tiene su propio usuario y PIN de perfil.
-- IPTV/FlujoTV: estos campos quedan NULL.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CustomerSubscriptions') AND name = 'ProfileUser'
)
BEGIN
    ALTER TABLE [dbo].[CustomerSubscriptions]
    ADD ProfileUser NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('CustomerSubscriptions') AND name = 'ProfilePin'
)
BEGIN
    ALTER TABLE [dbo].[CustomerSubscriptions]
    ADD ProfilePin NVARCHAR(20) NULL;
END
GO

-- ============================================================
-- SP: Asignar cliente — acepta ProfileUser y ProfilePin
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_IptvAccounts_AsignarCliente]
    @IptvAccountId  UNIQUEIDENTIFIER,
    @CustomerId     UNIQUEIDENTIFIER,
    @ExpirationDate DATETIME2,
    @TipoServicioId UNIQUEIDENTIFIER = NULL,
    @ProfileUser    NVARCHAR(100)    = NULL,
    @ProfilePin     NVARCHAR(20)     = NULL,
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
        (Id, IptvAccountId, CustomerId, TipoServicioId,
         ProfileUser, ProfilePin,
         StartDate, ExpirationDate, Status, CreatedAt)
    VALUES
        (@NewSubId, @IptvAccountId, @CustomerId, @TipoServicioId,
         @ProfileUser, @ProfilePin,
         GETDATE(), @ExpirationDate, 'Active', GETDATE());
END
GO

-- ============================================================
-- SP: Obtener cuentas con clientes — incluye ProfileUser/ProfilePin por slot
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
        a.AccessEmail,
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
        cs.ProfileUser,
        cs.ProfilePin,
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

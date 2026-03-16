-- ===========================================================
-- SCRIPT DE MIGRACIÓN: AccessEmail en IptvAccounts
--                      TieneCorreo en PlataformasConfig
-- Ejecutar en SQL Server Management Studio
-- ===========================================================

-- ── 1. Agregar columna AccessEmail a IptvAccounts ──────────
ALTER TABLE dbo.IptvAccounts
    ADD AccessEmail NVARCHAR(150) NULL;
GO

-- ── 2. Agregar columna TieneCorreo a PlataformasConfig ─────
ALTER TABLE dbo.PlataformasConfig
    ADD TieneCorreo BIT NOT NULL DEFAULT 0;
GO

-- ── 3. Marcar Netflix y Streaming como TieneCorreo = 1 ─────
UPDATE dbo.PlataformasConfig
SET TieneCorreo = 1
WHERE Plataforma IN ('Netflix', 'Streaming');
GO

-- ── 4. sp_IptvAccounts_Crear ───────────────────────────────
CREATE OR ALTER PROCEDURE dbo.sp_IptvAccounts_Crear
    @AccessUser     NVARCHAR(100),
    @AccessPassword NVARCHAR(100),
    @AccessEmail    NVARCHAR(150) = NULL,
    @PlatformUrl    NVARCHAR(255) = NULL,
    @PinCode        NVARCHAR(20)  = NULL,
    @TipoServicioId UNIQUEIDENTIFIER = NULL,
    @MaxSlots       INT           = 1,
    @PurchasePrice  DECIMAL(18,2) = 0,
    @ExpirationDate DATETIME      = NULL,
    @Notes          NVARCHAR(500) = NULL,
    @Id             UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();

    INSERT INTO dbo.IptvAccounts
        (Id, AccessUser, AccessPassword, AccessEmail, PlatformUrl, PinCode,
         TipoServicioId, MaxSlots, PurchasePrice, ExpirationDate, Notes, IsActive, CreatedAt)
    VALUES
        (@Id, @AccessUser, @AccessPassword, @AccessEmail, @PlatformUrl, @PinCode,
         @TipoServicioId, @MaxSlots, @PurchasePrice, @ExpirationDate, @Notes, 1, GETUTCDATE());
END;
GO

-- ── 5. sp_IptvAccounts_Actualizar ─────────────────────────
CREATE OR ALTER PROCEDURE dbo.sp_IptvAccounts_Actualizar
    @Id             UNIQUEIDENTIFIER,
    @AccessUser     NVARCHAR(100),
    @AccessPassword NVARCHAR(100),
    @AccessEmail    NVARCHAR(150) = NULL,
    @PlatformUrl    NVARCHAR(255) = NULL,
    @PinCode        NVARCHAR(20)  = NULL,
    @TipoServicioId UNIQUEIDENTIFIER = NULL,
    @MaxSlots       INT,
    @PurchasePrice  DECIMAL(18,2),
    @ExpirationDate DATETIME      = NULL,
    @Notes          NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.IptvAccounts
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
        UpdatedAt      = GETUTCDATE()
    WHERE Id = @Id;
END;
GO

-- ── 6. sp_IptvAccounts_ObtenerTodos ───────────────────────
-- Agregar AccessEmail al SELECT (si el SP usa SELECT * ya lo incluye,
-- pero si lista columnas explícitas, actualizar aquí).
-- Ejemplo si es SELECT *:
--   No requiere cambio.
-- Si lista columnas, agregar AccessEmail después de AccessPassword.

-- ── 7. sp_PlataformasConfig_Crear ─────────────────────────
CREATE OR ALTER PROCEDURE dbo.sp_PlataformasConfig_Crear
    @Plataforma     NVARCHAR(50),
    @NombreAmigable NVARCHAR(100),
    @LabelUsuario   NVARCHAR(50) = 'Usuario',
    @TieneUrl       BIT = 0,
    @TienePin       BIT = 0,
    @TieneCorreo    BIT = 0,
    @NewId          INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.PlataformasConfig
        (Plataforma, NombreAmigable, LabelUsuario, TieneUrl, TienePin, TieneCorreo, IsActive)
    VALUES
        (@Plataforma, @NombreAmigable, @LabelUsuario, @TieneUrl, @TienePin, @TieneCorreo, 1);

    SET @NewId = SCOPE_IDENTITY();
END;
GO

-- ── 8. sp_PlataformasConfig_Actualizar ────────────────────
CREATE OR ALTER PROCEDURE dbo.sp_PlataformasConfig_Actualizar
    @Id             INT,
    @Plataforma     NVARCHAR(50),
    @NombreAmigable NVARCHAR(100),
    @LabelUsuario   NVARCHAR(50),
    @TieneUrl       BIT,
    @TienePin       BIT,
    @TieneCorreo    BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.PlataformasConfig
    SET Plataforma     = @Plataforma,
        NombreAmigable = @NombreAmigable,
        LabelUsuario   = @LabelUsuario,
        TieneUrl       = @TieneUrl,
        TienePin       = @TienePin,
        TieneCorreo    = @TieneCorreo
    WHERE Id = @Id;
END;
GO

-- ── 9. sp_IptvAccounts_PorTipoServicio (actualizar) ───────
-- Ya incluye AccessEmail si se ejecutó el ALTER TABLE arriba.
-- Re-crear para asegurar que AccessEmail aparece en el SELECT.
CREATE OR ALTER PROCEDURE dbo.sp_IptvAccounts_PorTipoServicio
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
        st.Name                                         AS ServiceName,
        a.MaxSlots,
        a.PurchasePrice,
        a.ExpirationDate,
        DATEDIFF(DAY, GETUTCDATE(), a.ExpirationDate)   AS DaysRemaining,
        a.Notes,
        a.IsActive,
        a.CreatedAt,
        a.UpdatedAt,
        COUNT(s.Id)                                     AS UsedSlots
    FROM dbo.IptvAccounts a
    LEFT JOIN dbo.TiposServicio st
           ON st.Id = a.TipoServicioId
    LEFT JOIN dbo.CustomerSubscriptions s
           ON s.IptvAccountId = a.Id
          AND s.Status = 'Active'
    WHERE a.IsActive = 1
      AND a.TipoServicioId = @TipoServicioId
    GROUP BY
        a.Id, a.AccessUser, a.AccessPassword, a.AccessEmail, a.PlatformUrl,
        a.PinCode, a.TipoServicioId, st.Name, a.MaxSlots,
        a.PurchasePrice, a.ExpirationDate, a.Notes,
        a.IsActive, a.CreatedAt, a.UpdatedAt
    ORDER BY
        (a.MaxSlots - COUNT(s.Id)) DESC,
        a.ExpirationDate ASC;
END;
GO

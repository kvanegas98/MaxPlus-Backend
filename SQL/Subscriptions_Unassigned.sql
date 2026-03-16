-- ============================================================
-- Soporte para suscripciones sin cliente asignado (Unassigned)
-- ============================================================

-- 1. CustomerId ahora es nullable
ALTER TABLE [dbo].[CustomerSubscriptions]
    ALTER COLUMN CustomerId UNIQUEIDENTIFIER NULL;
GO

-- ============================================================
-- sp_Subscriptions_Crear  (actualizado: @CustomerId opcional)
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Subscriptions_Crear]
    @CustomerId       UNIQUEIDENTIFIER = NULL,
    @TipoServicioId   UNIQUEIDENTIFIER = NULL,
    @SubscriptionType NVARCHAR(20),
    @PlatformUrl      NVARCHAR(255)    = NULL,
    @AccessUser       NVARCHAR(100)    = NULL,
    @AccessPassword   NVARCHAR(100)    = NULL,
    @PinCode          NVARCHAR(20)     = NULL,
    @ExpirationDate   DATETIME2,
    @Id               UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();

    INSERT INTO [dbo].[CustomerSubscriptions]
        (Id, CustomerId, TipoServicioId, SubscriptionType,
         PlatformUrl, AccessUser, AccessPassword, PinCode,
         StartDate, ExpirationDate, Status)
    VALUES
        (@Id, @CustomerId, @TipoServicioId, @SubscriptionType,
         @PlatformUrl, @AccessUser, @AccessPassword, @PinCode,
         GETDATE(), @ExpirationDate,
         CASE WHEN @CustomerId IS NULL THEN 'Unassigned' ELSE 'Active' END);
END
GO

-- ============================================================
-- sp_Subscriptions_AsignarCliente
-- Asigna un cliente a una suscripción Unassigned → Active
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Subscriptions_AsignarCliente]
    @Id         UNIQUEIDENTIFIER,
    @CustomerId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[CustomerSubscriptions]
    SET CustomerId = @CustomerId,
        Status     = 'Active',
        UpdatedAt  = GETDATE()
    WHERE Id     = @Id
      AND Status = 'Unassigned';

    IF @@ROWCOUNT = 0
        THROW 50002, 'La suscripción no existe o ya tiene un cliente asignado.', 1;
END
GO

-- ============================================================
-- sp_Subscriptions_ObtenerSinAsignar
-- Lista suscripciones pendientes de asignar
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Subscriptions_ObtenerSinAsignar]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        cs.Id,
        cs.TipoServicioId,
        ts.Nombre   AS ServiceName,
        cs.SubscriptionType,
        cs.PlatformUrl,
        cs.AccessUser,
        cs.AccessPassword,
        cs.PinCode,
        cs.StartDate,
        cs.ExpirationDate,
        cs.Status,
        cs.CreatedAt
    FROM [dbo].[CustomerSubscriptions] cs
    LEFT JOIN [dbo].[TiposServicio] ts ON ts.Id = cs.TipoServicioId
    WHERE cs.Status = 'Unassigned'
    ORDER BY cs.CreatedAt DESC;
END
GO

-- ============================================================
-- sp_Subscriptions_ObtenerPorId
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Subscriptions_ObtenerPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        cs.Id,
        cs.CustomerId,
        cs.TipoServicioId,
        ts.Nombre       AS ServiceName,
        cs.SubscriptionType,
        cs.PlatformUrl,
        cs.AccessUser,
        cs.AccessPassword,
        cs.PinCode,
        cs.StartDate,
        cs.ExpirationDate,
        cs.Status,
        c.Nombre        AS CustomerName,
        c.Email         AS CustomerEmail,
        c.Telefono      AS CustomerPhone,
        cs.CreatedAt,
        cs.UpdatedAt
    FROM [dbo].[CustomerSubscriptions] cs
    LEFT JOIN [dbo].[Clientes]      c  ON c.Id  = cs.CustomerId
    LEFT JOIN [dbo].[TiposServicio] ts ON ts.Id = cs.TipoServicioId
    WHERE cs.Id = @Id;
END
GO

-- ============================================================
-- Logs del sistema en BD
-- ============================================================
CREATE TABLE [dbo].[SystemLogs] (
    [Id]        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SystemLogs_Id DEFAULT NEWID() PRIMARY KEY,
    [Level]     NVARCHAR(20)     NOT NULL DEFAULT 'Info',  -- Info | Warning | Error
    [Source]    NVARCHAR(100)    NOT NULL,
    [Message]   NVARCHAR(500)    NOT NULL,
    [Details]   NVARCHAR(MAX)    NULL,
    [CreatedAt] DATETIME2        NOT NULL DEFAULT GETDATE()
);
CREATE NONCLUSTERED INDEX IX_SystemLogs_CreatedAt ON [dbo].[SystemLogs] ([CreatedAt] DESC);
GO

-- Columnas de control de notificaciones en suscripciones
ALTER TABLE [dbo].[CustomerSubscriptions]
ADD [NotifiedAt7Days] DATETIME2 NULL,
    [NotifiedAt1Day]  DATETIME2 NULL;
GO

-- ============================================================
-- sp_Logs_Crear
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Logs_Crear]
    @Level   NVARCHAR(20),
    @Source  NVARCHAR(100),
    @Message NVARCHAR(500),
    @Details NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[SystemLogs] (Level, Source, Message, Details)
    VALUES (@Level, @Source, @Message, @Details);
END
GO

-- ============================================================
-- sp_Logs_ObtenerTodos
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Logs_ObtenerTodos]
    @Level  NVARCHAR(20) = NULL,
    @Source NVARCHAR(100) = NULL,
    @Top    INT = 200
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Top) Id, Level, Source, Message, Details, CreatedAt
    FROM [dbo].[SystemLogs]
    WHERE (@Level  IS NULL OR Level  = @Level)
      AND (@Source IS NULL OR Source = @Source)
    ORDER BY CreatedAt DESC;
END
GO

-- ============================================================
-- sp_CustomerSubscriptions_ObtenerPorVencer
-- Devuelve suscripciones activas que vencen en exactamente @DaysAhead días
-- con email del cliente (para notificar)
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_CustomerSubscriptions_ObtenerPorVencer]
    @DaysAhead INT  -- 7 o 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FechaDesde DATETIME2 = CAST(GETDATE() AS DATE);
    DECLARE @FechaHasta DATETIME2 = DATEADD(DAY, @DaysAhead, CAST(GETDATE() AS DATE));

    SELECT
        cs.Id,
        cs.CustomerId,
        c.Nombre    AS CustomerName,
        c.Email     AS CustomerEmail,
        c.Telefono  AS CustomerPhone,
        cs.TipoServicioId,
        ts.Nombre   AS ServiceName,
        cs.ExpirationDate,
        cs.Status,
        cs.NotifiedAt7Days,
        cs.NotifiedAt1Day
    FROM [dbo].[CustomerSubscriptions] cs
    INNER JOIN [dbo].[Clientes] c ON c.Id = cs.CustomerId
    LEFT JOIN  [dbo].[TiposServicio] ts ON ts.Id = cs.TipoServicioId
    WHERE cs.Status = 'Active'
      AND CAST(cs.ExpirationDate AS DATE) = CAST(@FechaHasta AS DATE)
      AND c.Email IS NOT NULL
      AND (
          (@DaysAhead = 7 AND cs.NotifiedAt7Days IS NULL)
          OR
          (@DaysAhead = 1 AND cs.NotifiedAt1Day  IS NULL)
      );
END
GO

-- ============================================================
-- sp_CustomerSubscriptions_MarcarNotificado
-- Registra que ya se envió la notificación de vencimiento
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_CustomerSubscriptions_MarcarNotificado]
    @Id        UNIQUEIDENTIFIER,
    @DaysAhead INT  -- 7 o 1
AS
BEGIN
    SET NOCOUNT ON;
    IF @DaysAhead = 7
        UPDATE [dbo].[CustomerSubscriptions] SET NotifiedAt7Days = GETDATE() WHERE Id = @Id;
    ELSE IF @DaysAhead = 1
        UPDATE [dbo].[CustomerSubscriptions] SET NotifiedAt1Day  = GETDATE() WHERE Id = @Id;
END
GO

-- ============================================================
-- sp_CustomerSubscriptions_ExpirarVencidas
-- Marca como Expired las suscripciones activas que ya vencieron
-- Retorna cuántas se marcaron
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_CustomerSubscriptions_ExpirarVencidas]
    @Cantidad INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[CustomerSubscriptions]
    SET    Status    = 'Expired',
           UpdatedAt = GETDATE()
    WHERE  Status         = 'Active'
      AND  ExpirationDate < GETDATE();

    SET @Cantidad = @@ROWCOUNT;
END
GO

-- ============================================================
-- sp_CustomerSubscriptions_Renovar
-- Marca la suscripción actual como Renewed y crea una nueva
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_CustomerSubscriptions_Renovar]
    @SubscriptionId UNIQUEIDENTIFIER,
    @NewExpiration  DATETIME2,
    @NewId          UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validar que existe y está activa o expirada (no cancelada)
    IF NOT EXISTS (
        SELECT 1 FROM [dbo].[CustomerSubscriptions]
        WHERE Id = @SubscriptionId AND Status IN ('Active', 'Expired')
    )
        THROW 50001, 'La suscripción no existe o ya fue cancelada.', 1;

    -- Marcar anterior como renovada
    UPDATE [dbo].[CustomerSubscriptions]
    SET Status    = 'Renewed',
        UpdatedAt = GETDATE()
    WHERE Id = @SubscriptionId;

    -- Crear nueva suscripción heredando todos los datos
    SET @NewId = NEWID();
    INSERT INTO [dbo].[CustomerSubscriptions]
        (Id, CustomerId, TipoServicioId, SubscriptionType,
         PlatformUrl, AccessUser, AccessPassword, PinCode,
         StartDate, ExpirationDate, Status)
    SELECT
        @NewId, CustomerId, TipoServicioId, SubscriptionType,
        PlatformUrl, AccessUser, AccessPassword, PinCode,
        GETDATE(), @NewExpiration, 'Active'
    FROM [dbo].[CustomerSubscriptions]
    WHERE Id = @SubscriptionId;
END
GO

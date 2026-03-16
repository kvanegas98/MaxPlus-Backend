-- ============================================================
-- Solo SPs del job de suscripciones (tabla y columnas ya existen)
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

CREATE OR ALTER PROCEDURE [dbo].[sp_Logs_ObtenerTodos]
    @Level  NVARCHAR(20)  = NULL,
    @Source NVARCHAR(100) = NULL,
    @Top    INT           = 200
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

CREATE OR ALTER PROCEDURE [dbo].[sp_CustomerSubscriptions_ObtenerPorVencer]
    @DaysAhead INT
AS
BEGIN
    SET NOCOUNT ON;

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
      AND CAST(cs.ExpirationDate AS DATE) = CAST(DATEADD(DAY, @DaysAhead, GETDATE()) AS DATE)
      AND c.Telefono IS NOT NULL
      AND (
          (@DaysAhead = 7 AND cs.NotifiedAt7Days IS NULL)
          OR
          (@DaysAhead = 1 AND cs.NotifiedAt1Day  IS NULL)
      );
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_CustomerSubscriptions_MarcarNotificado]
    @Id        UNIQUEIDENTIFIER,
    @DaysAhead INT
AS
BEGIN
    SET NOCOUNT ON;
    IF @DaysAhead = 7
        UPDATE [dbo].[CustomerSubscriptions] SET NotifiedAt7Days = GETDATE() WHERE Id = @Id;
    ELSE IF @DaysAhead = 1
        UPDATE [dbo].[CustomerSubscriptions] SET NotifiedAt1Day  = GETDATE() WHERE Id = @Id;
END
GO

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

CREATE OR ALTER PROCEDURE [dbo].[sp_CustomerSubscriptions_Renovar]
    @SubscriptionId UNIQUEIDENTIFIER,
    @NewExpiration  DATETIME2,
    @NewId          UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM [dbo].[CustomerSubscriptions]
        WHERE Id = @SubscriptionId AND Status IN ('Active', 'Expired')
    )
        THROW 50001, 'La suscripción no existe o ya fue cancelada.', 1;

    UPDATE [dbo].[CustomerSubscriptions]
    SET Status    = 'Renewed',
        UpdatedAt = GETDATE()
    WHERE Id = @SubscriptionId;

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

-- ============================================================
-- TABLA: ServiceOrders
-- ============================================================
CREATE TABLE [dbo].[ServiceOrders] (
    [Id]              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ServiceOrders_Id DEFAULT NEWID() PRIMARY KEY,
    [CustomerName]    NVARCHAR(150)    NOT NULL,
    [CustomerPhone]   NVARCHAR(20)     NULL,
    [CustomerEmail]   NVARCHAR(100)    NULL,
    [TipoServicioId]  UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [dbo].[TiposServicio]([Id]),
    [Notes]           NVARCHAR(500)    NULL,
    [Status]          NVARCHAR(20)     NOT NULL DEFAULT 'Pending', -- Pending | Approved | Rejected
    [ApprovedBy]      UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [dbo].[Usuarios]([Id]),
    [ApprovedAt]      DATETIME2        NULL,
    [SubscriptionId]  UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [dbo].[CustomerSubscriptions]([Id]),
    [RejectionReason] NVARCHAR(255)    NULL,
    [IpAddress]       NVARCHAR(45)     NULL,
    [CreatedAt]       DATETIME2        NOT NULL DEFAULT GETDATE()
);

CREATE NONCLUSTERED INDEX [IX_ServiceOrders_Status] ON [dbo].[ServiceOrders] ([Status]);
GO

-- ============================================================
-- sp_ServiceOrders_Crear
-- ============================================================
CREATE OR ALTER PROCEDURE sp_ServiceOrders_Crear
    @CustomerName   NVARCHAR(150),
    @CustomerPhone  NVARCHAR(20)     = NULL,
    @CustomerEmail  NVARCHAR(100)    = NULL,
    @TipoServicioId UNIQUEIDENTIFIER = NULL,
    @Notes          NVARCHAR(500)    = NULL,
    @IpAddress      NVARCHAR(45)     = NULL,
    @Id             UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();

    INSERT INTO [dbo].[ServiceOrders] (Id, CustomerName, CustomerPhone, CustomerEmail, TipoServicioId, Notes, IpAddress)
    VALUES (@Id, @CustomerName, @CustomerPhone, @CustomerEmail, @TipoServicioId, @Notes, @IpAddress);
END
GO

-- ============================================================
-- sp_ServiceOrders_ObtenerTodos
-- ============================================================
CREATE OR ALTER PROCEDURE sp_ServiceOrders_ObtenerTodos
    @Status NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        o.Id, o.CustomerName, o.CustomerPhone, o.CustomerEmail,
        o.TipoServicioId, ts.Nombre AS ServiceName,
        o.Notes, o.Status,
        o.ApprovedBy, o.ApprovedAt, o.SubscriptionId,
        o.RejectionReason, o.IpAddress, o.CreatedAt
    FROM [dbo].[ServiceOrders] o
    LEFT JOIN [dbo].[TiposServicio] ts ON ts.Id = o.TipoServicioId
    WHERE (@Status IS NULL OR o.Status = @Status)
    ORDER BY o.CreatedAt DESC;
END
GO

-- ============================================================
-- sp_ServiceOrders_ObtenerPorId
-- ============================================================
CREATE OR ALTER PROCEDURE sp_ServiceOrders_ObtenerPorId
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        o.Id, o.CustomerName, o.CustomerPhone, o.CustomerEmail,
        o.TipoServicioId, ts.Nombre AS ServiceName,
        o.Notes, o.Status,
        o.ApprovedBy, o.ApprovedAt, o.SubscriptionId,
        o.RejectionReason, o.IpAddress, o.CreatedAt
    FROM [dbo].[ServiceOrders] o
    LEFT JOIN [dbo].[TiposServicio] ts ON ts.Id = o.TipoServicioId
    WHERE o.Id = @Id;
END
GO

-- ============================================================
-- sp_ServiceOrders_Aprobar
-- ============================================================
CREATE OR ALTER PROCEDURE sp_ServiceOrders_Aprobar
    @Id             UNIQUEIDENTIFIER,
    @ApprovedBy     UNIQUEIDENTIFIER,
    @SubscriptionId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[ServiceOrders]
    SET Status         = 'Approved',
        ApprovedBy     = @ApprovedBy,
        ApprovedAt     = GETDATE(),
        SubscriptionId = @SubscriptionId
    WHERE Id = @Id;
END
GO

-- ============================================================
-- sp_ServiceOrders_Rechazar
-- ============================================================
CREATE OR ALTER PROCEDURE sp_ServiceOrders_Rechazar
    @Id              UNIQUEIDENTIFIER,
    @ApprovedBy      UNIQUEIDENTIFIER,
    @RejectionReason NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[ServiceOrders]
    SET Status          = 'Rejected',
        ApprovedBy      = @ApprovedBy,
        ApprovedAt      = GETDATE(),
        RejectionReason = @RejectionReason
    WHERE Id = @Id;
END
GO

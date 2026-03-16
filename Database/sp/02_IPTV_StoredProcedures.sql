-- =========================================================================================
-- MAXPLUS IPTV BASE SCHEMA (STORED PROCEDURES)
-- =========================================================================================

-- ==========================================================
-- 1. USUARIOS Y ROLES
-- ==========================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Roles_ObtenerTodos]
AS
BEGIN
    SELECT * FROM [dbo].[Roles];
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Usuarios_Crear]
    @FullName NVARCHAR(100),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @RoleId UNIQUEIDENTIFIER,
    @IsActive BIT,
    @Id UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();
    INSERT INTO [dbo].[Usuarios] (Id, FullName, Email, PasswordHash, RoleId, IsActive)
    VALUES (@Id, @FullName, @Email, @PasswordHash, @RoleId, @IsActive);
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Usuarios_Actualizar]
    @Id UNIQUEIDENTIFIER,
    @FullName NVARCHAR(100),
    @Email NVARCHAR(100),
    @RoleId UNIQUEIDENTIFIER,
    @IsActive BIT
AS
BEGIN
    UPDATE [dbo].[Usuarios]
    SET FullName = @FullName, Email = @Email, RoleId = @RoleId, IsActive = @IsActive
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Usuarios_ObtenerTodos]
AS
BEGIN
    SELECT u.*, r.Nombre as RoleName 
    FROM [dbo].[Usuarios] u
    INNER JOIN [dbo].[Roles] r ON u.RoleId = r.Id;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Usuarios_ObtenerPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SELECT u.*, r.Nombre as RoleName 
    FROM [dbo].[Usuarios] u
    INNER JOIN [dbo].[Roles] r ON u.RoleId = r.Id
    WHERE u.Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Usuarios_ObtenerPorEmail]
    @Email NVARCHAR(100)
AS
BEGIN
    SELECT u.*, r.Nombre as RoleName 
    FROM [dbo].[Usuarios] u
    INNER JOIN [dbo].[Roles] r ON u.RoleId = r.Id
    WHERE u.Email = @Email;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Usuarios_CambiarPassword]
    @Id UNIQUEIDENTIFIER,
    @NewPasswordHash NVARCHAR(255)
AS
BEGIN
    UPDATE [dbo].[Usuarios] SET PasswordHash = @NewPasswordHash WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Usuarios_DesactivarPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    UPDATE [dbo].[Usuarios] SET IsActive = 0 WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Usuarios_ActualizarUltimoLogin]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    UPDATE [dbo].[Usuarios] SET LastLoginAt = GETDATE() WHERE Id = @Id;
END;
GO


-- ==========================================================
-- 2. CLIENTES
-- ==========================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Clientes_Crear]
    @Nombre NVARCHAR(150),
    @Telefono NVARCHAR(20),
    @Direccion NVARCHAR(255),
    @Email NVARCHAR(100) = NULL,
    @Id UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();
    INSERT INTO [dbo].[Clientes] (Id, Nombre, Telefono, Direccion, Email, IsActive)
    VALUES (@Id, @Nombre, @Telefono, @Direccion, @Email, 1);
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Clientes_Actualizar]
    @Id UNIQUEIDENTIFIER,
    @Nombre NVARCHAR(150),
    @Telefono NVARCHAR(20),
    @Direccion NVARCHAR(255),
    @Email NVARCHAR(100) = NULL
AS
BEGIN
    UPDATE [dbo].[Clientes]
    SET Nombre = @Nombre, Telefono = @Telefono, Direccion = @Direccion, Email = @Email, UpdatedAt = GETDATE()
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Clientes_ObtenerTodos]
AS
BEGIN
    SELECT * FROM [dbo].[Clientes] WHERE IsActive = 1 ORDER BY Nombre;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Clientes_ObtenerPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SELECT * FROM [dbo].[Clientes] WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Clientes_Buscar]
    @Termino NVARCHAR(100)
AS
BEGIN
    SELECT * FROM [dbo].[Clientes] 
    WHERE IsActive = 1 AND (Nombre LIKE '%' + @Termino + '%' OR Telefono LIKE '%' + @Termino + '%')
    ORDER BY Nombre;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Clientes_Desactivar]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    UPDATE [dbo].[Clientes] SET IsActive = 0, UpdatedAt = GETDATE() WHERE Id = @Id;
END;
GO


-- ==========================================================
-- 3. CONFIGURACION
-- ==========================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Configuracion_Obtener]
AS
BEGIN
    SELECT * FROM [dbo].[Configuracion] WHERE Id = 1;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Configuracion_Actualizar]
    @NombreNegocio NVARCHAR(200),
    @Telefono NVARCHAR(50),
    @Descripcion NVARCHAR(500),
    @Direccion NVARCHAR(255),
    @LogoUrl NVARCHAR(500),
    @TasaCambioUSD DECIMAL(18,4),
    @DemoPhpBaseUrl NVARCHAR(500),
    @MenuPublicoHabilitado BIT,
    @DemoAutoApprove BIT
AS
BEGIN
    UPDATE [dbo].[Configuracion]
    SET NombreNegocio = @NombreNegocio, Telefono = @Telefono, Descripcion = @Descripcion, 
        Direccion = @Direccion, LogoUrl = @LogoUrl,
        TasaCambioUSD = @TasaCambioUSD, DemoPhpBaseUrl = @DemoPhpBaseUrl,
        MenuPublicoHabilitado = @MenuPublicoHabilitado, DemoAutoApprove = @DemoAutoApprove,
        UpdatedAt = GETDATE()
    WHERE Id = 1;
END;
GO


-- ==========================================================
-- 4. TIPOS DE SERVICIO (CATÁLOGO)
-- ==========================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_ObtenerTodos]
AS
BEGIN
    SELECT * FROM [dbo].[TiposServicio] WHERE IsActive = 1 ORDER BY Nombre;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_ObtenerPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SELECT * FROM [dbo].[TiposServicio] WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_Crear]
    @Nombre NVARCHAR(150),
    @Descripcion NVARCHAR(500),
    @Precio DECIMAL(18,2),
    @PrecioCompra DECIMAL(18,2),
    @DurationDays INT,
    @Category NVARCHAR(50),
    @ImageUrl NVARCHAR(500) = NULL,
    @Id UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();
    INSERT INTO [dbo].[TiposServicio] (Id, Nombre, Descripcion, Precio, PrecioCompra, DurationDays, Category, ImageUrl)
    VALUES (@Id, @Nombre, @Descripcion, @Precio, @PrecioCompra, @DurationDays, @Category, @ImageUrl);
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_Actualizar]
    @Id UNIQUEIDENTIFIER,
    @Nombre NVARCHAR(150),
    @Descripcion NVARCHAR(500),
    @Precio DECIMAL(18,2),
    @PrecioCompra DECIMAL(18,2),
    @DurationDays INT,
    @Category NVARCHAR(50),
    @ImageUrl NVARCHAR(500) = NULL
AS
BEGIN
    UPDATE [dbo].[TiposServicio]
    SET Nombre = @Nombre, Descripcion = @Descripcion, Precio = @Precio, PrecioCompra = @PrecioCompra,
        DurationDays = @DurationDays, Category = @Category, ImageUrl = @ImageUrl
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_TiposServicio_Desactivar]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    UPDATE [dbo].[TiposServicio] SET IsActive = 0 WHERE Id = @Id;
END;
GO




-- ==========================================================
-- 6. SUSCRIPCIONES DE CLIENTES
-- ==========================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Subscriptions_Crear]
    @CustomerId UNIQUEIDENTIFIER,
    @TipoServicioId UNIQUEIDENTIFIER = NULL,
    @SubscriptionType NVARCHAR(50),
    @PlatformUrl NVARCHAR(255) = NULL,
    @AccessUser NVARCHAR(100) = NULL,
    @AccessPassword NVARCHAR(100) = NULL,
    @PinCode NVARCHAR(20) = NULL,
    @ExpirationDate DATETIME2,
    @Id UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = NEWID();
    INSERT INTO [dbo].[CustomerSubscriptions] 
        (Id, CustomerId, TipoServicioId, SubscriptionType, PlatformUrl, 
         AccessUser, AccessPassword, PinCode, StartDate, ExpirationDate, Status)
    VALUES 
        (@Id, @CustomerId, @TipoServicioId, @SubscriptionType, @PlatformUrl,
         @AccessUser, @AccessPassword, @PinCode, GETDATE(), @ExpirationDate, 'Active');
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Subscriptions_ObtenerPorCliente]
    @CustomerId UNIQUEIDENTIFIER
AS
BEGIN
    SELECT s.*, ts.Nombre AS ServiceName
    FROM [dbo].[CustomerSubscriptions] s
    LEFT JOIN [dbo].[TiposServicio] ts ON s.TipoServicioId = ts.Id
    WHERE s.CustomerId = @CustomerId
    ORDER BY s.CreatedAt DESC;
END;
GO



CREATE OR ALTER PROCEDURE [dbo].[sp_Subscriptions_ObtenerActivas]
AS
BEGIN
    SELECT s.*, c.Nombre AS CustomerName, ts.Nombre AS ServiceName
    FROM [dbo].[CustomerSubscriptions] s
    INNER JOIN [dbo].[Clientes] c ON s.CustomerId = c.Id
    LEFT JOIN [dbo].[TiposServicio] ts ON s.TipoServicioId = ts.Id
    WHERE s.Status = 'Active'
    ORDER BY s.ExpirationDate ASC;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Subscriptions_Actualizar]
    @Id UNIQUEIDENTIFIER,
    @PlatformUrl NVARCHAR(255) = NULL,
    @AccessUser NVARCHAR(100) = NULL,
    @AccessPassword NVARCHAR(100) = NULL,
    @PinCode NVARCHAR(20) = NULL,
    @ExpirationDate DATETIME2,
    @Status NVARCHAR(50)
AS
BEGIN
    UPDATE [dbo].[CustomerSubscriptions]
    SET PlatformUrl = @PlatformUrl,
        AccessUser = @AccessUser, AccessPassword = @AccessPassword, PinCode = @PinCode,
        ExpirationDate = @ExpirationDate, Status = @Status, UpdatedAt = GETDATE()
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Subscriptions_Cancelar]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    UPDATE [dbo].[CustomerSubscriptions] SET Status = 'Cancelled', UpdatedAt = GETDATE() WHERE Id = @Id;
END;
GO


-- ==========================================================
-- 7. FACTURAS / VENTAS (SOLO CONTADO)
-- ==========================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ProcesarVenta]
    @NombreCliente NVARCHAR(150),
    @UsuarioId UNIQUEIDENTIFIER,
    @TotalAmount DECIMAL(18,2),
    @DetailsJson NVARCHAR(MAX),
    @TipoOrden NVARCHAR(50),
    @MetodoPago NVARCHAR(50),
    @MontoRecibido DECIMAL(18,2),
    @ClienteId UNIQUEIDENTIFIER = NULL,
    @MontoDescuento DECIMAL(18,2) = 0,
    @Nota NVARCHAR(500) = NULL,
    @SubscriptionId UNIQUEIDENTIFIER = NULL,
    @FacturaId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @FacturaId = NEWID();

    DECLARE @NumeroOrden NVARCHAR(50) = 'ORD-' + RIGHT(CAST(@FacturaId AS VARCHAR(36)), 8);
    DECLARE @PublicLinkToken NVARCHAR(50) = REPLACE(CAST(NEWID() AS VARCHAR(36)), '-', '');

    INSERT INTO [dbo].[Facturas] (
        Id, NumeroOrden, CustomerName, CustomerId, UserId, OrderType, PaymentMethod,
        AmountReceived, DiscountAmount, TotalAmount, Status, SaleDate, Nota, PublicLinkToken, SubscriptionId
    )
    VALUES (
        @FacturaId, @NumeroOrden, @NombreCliente, @ClienteId, @UsuarioId, @TipoOrden, @MetodoPago,
        @MontoRecibido, @MontoDescuento, @TotalAmount, 'Pagada', GETDATE(), @Nota, @PublicLinkToken, @SubscriptionId
    );

    INSERT INTO [dbo].[InvoiceDetails] (Id, InvoiceId, TipoServicioId, Concept, Quantity, UnitPrice, DiscountAmount, SubTotal, Nota)
    SELECT NEWID(), @FacturaId, 
           CASE WHEN JSON_VALUE(d.value, '$.TipoServicioId') = '' THEN NULL ELSE JSON_VALUE(d.value, '$.TipoServicioId') END,
           JSON_VALUE(d.value, '$.Concepto'),
           CAST(JSON_VALUE(d.value, '$.Cantidad') AS INT),
           CAST(JSON_VALUE(d.value, '$.PrecioUnitario') AS DECIMAL(18,2)),
           CAST(JSON_VALUE(d.value, '$.Descuento') AS DECIMAL(18,2)),
           CAST(JSON_VALUE(d.value, '$.SubTotal') AS DECIMAL(18,2)),
           JSON_VALUE(d.value, '$.Nota')
    FROM OPENJSON(@DetailsJson) d;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ObtenerFacturaPorId]
    @FacturaId UNIQUEIDENTIFIER
AS
BEGIN
    SELECT * FROM [dbo].[Facturas] WHERE Id = @FacturaId;
    SELECT * FROM [dbo].[InvoiceDetails] WHERE InvoiceId = @FacturaId;
    SELECT * FROM [dbo].[Clientes] WHERE Id = (SELECT CustomerId FROM [dbo].[Facturas] WHERE Id = @FacturaId);
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Facturas_Anular]
    @FacturaId UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER,
    @Motivo NVARCHAR(255)
AS
BEGIN
    DECLARE @Status NVARCHAR(50);
    SELECT @Status = Status FROM [dbo].[Facturas] WHERE Id = @FacturaId;

    IF @Status = 'Anulada' RETURN;

    UPDATE [dbo].[Facturas] 
    SET Status = 'Anulada', IsVoided = 1, VoidedAt = GETDATE(), VoidedBy = @UsuarioId, VoidReason = @Motivo
    WHERE Id = @FacturaId;
END;
GO


-- ==========================================================
-- 8. REPORTES Y ESTADÍSTICAS
-- ==========================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Reportes_Resumen]
    @Filtro NVARCHAR(50)
AS
BEGIN
    DECLARE @Inicio DATETIME2 = CASE WHEN @Filtro = 'Semana' THEN DATEADD(day, -7, GETDATE()) ELSE CAST(GETDATE() AS DATE) END;
    
    SELECT 
        ISNULL(SUM(TotalAmount), 0) AS VentasTotales,
        ISNULL(SUM(TotalAmount)/NULLIF(COUNT(Id), 0), 0) AS TicketPromedio,
        COUNT(Id) AS TotalOrdenes,
        (SELECT TOP 1 Concept FROM InvoiceDetails d INNER JOIN Facturas f ON d.InvoiceId = f.Id WHERE f.SaleDate >= @Inicio AND f.IsVoided=0 GROUP BY Concept ORDER BY SUM(Quantity) DESC) AS ProductoMasVendido
    FROM Facturas 
    WHERE SaleDate >= @Inicio AND IsVoided = 0;

    SELECT OrderType AS TipoOrden, COUNT(Id) AS TotalOrdenes, SUM(TotalAmount) AS TotalVentas
    FROM Facturas WHERE SaleDate >= @Inicio AND IsVoided = 0 GROUP BY OrderType;

    -- Conteo de demos aprobadas hoy
    SELECT COUNT(Id) AS TotalDemosHoy
    FROM DemoRequests WHERE CAST(CreatedAt AS DATE) >= CAST(@Inicio AS DATE) AND Status = 'Approved';

    -- Suscripciones activas totales
    SELECT COUNT(Id) AS SuscripcionesActivas
    FROM CustomerSubscriptions WHERE Status = 'Active';
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Reportes_VentasPorHora]
AS
BEGIN
    SELECT DATEPART(HOUR, SaleDate) AS Hora, SUM(TotalAmount) AS TotalVentas, COUNT(Id) AS TotalOrdenes
    FROM Facturas WHERE CAST(SaleDate AS DATE) = CAST(GETDATE() AS DATE) AND IsVoided = 0
    GROUP BY DATEPART(HOUR, SaleDate) ORDER BY Hora;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Reportes_TopProductos]
    @Filtro NVARCHAR(50),
    @Top INT = 5
AS
BEGIN
    DECLARE @Inicio DATETIME2 = CASE WHEN @Filtro = 'Semana' THEN DATEADD(day, -7, GETDATE()) ELSE CAST(GETDATE() AS DATE) END;
    SELECT TOP (@Top) d.Concept AS Nombre, SUM(d.Quantity) AS UnidadesVendidas, SUM(d.SubTotal) AS TotalRecaudado
    FROM InvoiceDetails d INNER JOIN Facturas f ON d.InvoiceId = f.Id
    WHERE f.SaleDate >= @Inicio AND f.IsVoided = 0
    GROUP BY d.Concept ORDER BY UnidadesVendidas DESC;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Reportes_HistorialOrdenes]
    @FechaInicio DATETIME2 = NULL, @FechaFin DATETIME2 = NULL, @PageNumber INT = 1, @PageSize INT = 10, @Cliente NVARCHAR(150) = NULL, @NumeroOrden NVARCHAR(50) = NULL
AS
BEGIN
    SELECT Id, NumeroOrden, SaleDate AS FechaVenta, CustomerName AS NombreCliente, CustomerId,
           (SELECT Nombre FROM Clientes WHERE Id = Facturas.CustomerId) AS ClienteNombre,
           (SELECT SUM(Quantity) FROM InvoiceDetails WHERE InvoiceId = Facturas.Id) AS TotalProductos,
           TotalAmount, Status AS Estado, IsVoided, OrderType AS TipoOrden, PaymentMethod AS MetodoPago
    FROM Facturas
    WHERE (@FechaInicio IS NULL OR SaleDate >= @FechaInicio)
      AND (@FechaFin IS NULL OR SaleDate <= @FechaFin)
      AND (@Cliente IS NULL OR CustomerName LIKE '%' + @Cliente + '%')
      AND (@NumeroOrden IS NULL OR NumeroOrden LIKE '%' + @NumeroOrden + '%')
    ORDER BY SaleDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT * FROM InvoiceDetails WHERE InvoiceId IN (
        SELECT Id FROM Facturas
        WHERE (@FechaInicio IS NULL OR SaleDate >= @FechaInicio)
          AND (@FechaFin IS NULL OR SaleDate <= @FechaFin)
          AND (@Cliente IS NULL OR CustomerName LIKE '%' + @Cliente + '%')
          AND (@NumeroOrden IS NULL OR NumeroOrden LIKE '%' + @NumeroOrden + '%')
        ORDER BY SaleDate DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
    );
END;
GO

-- ==========================================================
-- 9. DEMO REQUESTS (PRUEBAS GRATUITAS)
-- ==========================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_DemoRequests_Crear]
    @CustomerName NVARCHAR(150),
    @CustomerPhone NVARCHAR(20),
    @CustomerEmail NVARCHAR(100) = NULL,
    @IpAddress NVARCHAR(50),
    @Country NVARCHAR(100),
    @PhoneVerificationCode NVARCHAR(6),
    @TipoServicioId UNIQUEIDENTIFIER,
    @Id UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET @Id = NEWID();
    INSERT INTO [dbo].[DemoRequests] (
        [Id], [CustomerName], [CustomerPhone], [CustomerEmail], [TipoServicioId],
        [IpAddress], [Country], [PhoneVerificationCode], [IsPhoneVerified], [Status], [CreatedAt]
    )
    VALUES (
        @Id, @CustomerName, @CustomerPhone, @CustomerEmail, @TipoServicioId,
        @IpAddress, @Country, @PhoneVerificationCode, 0, 'Pending', GETDATE()
    );
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_DemoRequests_VerificarTelefono]
    @Id UNIQUEIDENTIFIER,
    @Code NVARCHAR(6)
AS
BEGIN
    IF EXISTS(SELECT 1 FROM [dbo].[DemoRequests] WHERE [Id] = @Id AND [PhoneVerificationCode] = @Code)
    BEGIN
        UPDATE [dbo].[DemoRequests] 
        SET [IsPhoneVerified] = 1 
        WHERE [Id] = @Id;
    END
    ELSE
    BEGIN
        THROW 50000, 'Código de verificación incorrecto', 1;
    END
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_DemoRequests_ObtenerPorId]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SELECT dr.*, ts.Nombre AS ServiceName
    FROM [dbo].[DemoRequests] dr
    LEFT JOIN [dbo].[TiposServicio] ts ON dr.TipoServicioId = ts.Id
    WHERE dr.Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_DemoRequests_ObtenerTodos]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SELECT dr.*, ts.Nombre AS ServiceName
    FROM [dbo].[DemoRequests] dr
    LEFT JOIN [dbo].[TiposServicio] ts ON dr.TipoServicioId = ts.Id
    WHERE (@Status IS NULL OR dr.Status = @Status)
    ORDER BY dr.CreatedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_DemoRequests_ObtenerPendientes]
AS
BEGIN
    SELECT * FROM [dbo].[DemoRequests] 
    WHERE [Status] = 'Pending' AND [IsPhoneVerified] = 1
    ORDER BY [CreatedAt] ASC;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_DemoRequests_Aprobar]
    @Id UNIQUEIDENTIFIER,
    @DemoUrl NVARCHAR(500),
    @ResponseHtml NVARCHAR(MAX),
    @ApprovedBy UNIQUEIDENTIFIER = NULL
AS
BEGIN
    UPDATE [dbo].[DemoRequests]
    SET [Status] = 'Approved',
        [DemoUrl] = @DemoUrl,
        [ResponseHtml] = @ResponseHtml,
        [ApprovedBy] = @ApprovedBy,
        [ApprovedAt] = GETDATE(),
        [ExpiresAt] = DATEADD(hour, 24, GETDATE())
    WHERE [Id] = @Id;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_DemoRequests_Rechazar]
    @Id UNIQUEIDENTIFIER,
    @RejectionReason NVARCHAR(255),
    @ApprovedBy UNIQUEIDENTIFIER
AS
BEGIN
    UPDATE [dbo].[DemoRequests]
    SET [Status] = 'Rejected',
        [RejectionReason] = @RejectionReason,
        [ApprovedBy] = @ApprovedBy,
        [ApprovedAt] = GETDATE()
    WHERE [Id] = @Id;
END;
GO

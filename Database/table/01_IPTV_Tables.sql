-- =========================================================================================
-- MAXPLUS IPTV BASE SCHEMA (TABLES)
-- =========================================================================================
-- Use this script to create the tables in a clean database.

CREATE TABLE [dbo].[Roles] (
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Roles_Id DEFAULT NEWID() PRIMARY KEY,
    [Nombre] NVARCHAR(50) NOT NULL UNIQUE,
    [Descripcion] NVARCHAR(200) NULL
);

CREATE TABLE [dbo].[Usuarios] (
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Usuarios_Id DEFAULT NEWID() PRIMARY KEY,
    [FullName] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(100) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(255) NOT NULL,
    [RoleId] UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [dbo].[Roles]([Id]),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [LastLoginAt] DATETIME2 NULL
);

CREATE TABLE [dbo].[TiposServicio] (
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TiposServicio_Id DEFAULT NEWID() PRIMARY KEY,
    [Nombre] NVARCHAR(150) NOT NULL,
    [Descripcion] NVARCHAR(500) NULL,
    [Precio] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [PrecioCompra] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [DurationDays] INT NOT NULL DEFAULT 30,
    [Category] NVARCHAR(50) NOT NULL DEFAULT 'Paid',   -- Paid | Demo
    [ImageUrl] NVARCHAR(500) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE()
);

CREATE TABLE [dbo].[Clientes] (
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Clientes_Id DEFAULT NEWID() PRIMARY KEY,
    [Nombre] NVARCHAR(150) NOT NULL,
    [Telefono] NVARCHAR(20) NULL,
    [Direccion] NVARCHAR(255) NULL,
    [Email] NVARCHAR(100) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NULL
);



CREATE TABLE [dbo].[CustomerSubscriptions] (
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_CustSub_Id DEFAULT NEWID() PRIMARY KEY,
    [CustomerId] UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [dbo].[Clientes]([Id]),
    [TipoServicioId] UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [dbo].[TiposServicio]([Id]),
    [SubscriptionType] NVARCHAR(50) NOT NULL DEFAULT 'Paid',  -- Paid | Demo
    [PlatformUrl] NVARCHAR(255) NULL,
    [AccessUser] NVARCHAR(100) NULL,
    [AccessPassword] NVARCHAR(100) NULL,
    [PinCode] NVARCHAR(20) NULL,
    [StartDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [ExpirationDate] DATETIME2 NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',  -- Active | Expired | Cancelled
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NULL
);

CREATE NONCLUSTERED INDEX [IX_CustSub_CustomerId] ON [dbo].[CustomerSubscriptions] ([CustomerId]);
CREATE NONCLUSTERED INDEX [IX_CustSub_Status] ON [dbo].[CustomerSubscriptions] ([Status]);


CREATE TABLE [dbo].[Facturas] (
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Facturas_Id DEFAULT NEWID() PRIMARY KEY,
    [NumeroOrden] NVARCHAR(50) NOT NULL UNIQUE,
    [CustomerName] NVARCHAR(150) NOT NULL,
    [CustomerId] UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [dbo].[Clientes]([Id]),
    [UserId] UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [dbo].[Usuarios]([Id]),
    [OrderType] NVARCHAR(50) NOT NULL DEFAULT 'Venta',
    [PaymentMethod] NVARCHAR(50) NULL,
    [AmountReceived] DECIMAL(18,2) NULL,
    [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [TotalAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pagada',  -- Pagada | Anulada
    [SaleDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [Nota] NVARCHAR(500) NULL,
    [PublicLinkToken] NVARCHAR(50) NOT NULL UNIQUE,
    [SubscriptionId] UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [dbo].[CustomerSubscriptions]([Id]),
    [IsVoided] BIT NOT NULL DEFAULT 0,
    [VoidedAt] DATETIME2 NULL,
    [VoidedBy] UNIQUEIDENTIFIER NULL,
    [VoidReason] NVARCHAR(255) NULL
);

CREATE TABLE [dbo].[InvoiceDetails] (
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_InvoiceDetails_Id DEFAULT NEWID() PRIMARY KEY,
    [InvoiceId] UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [dbo].[Facturas]([Id]),
    [TipoServicioId] UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [dbo].[TiposServicio]([Id]),
    [Concept] NVARCHAR(200) NOT NULL,
    [Quantity] INT NOT NULL DEFAULT 1,
    [UnitPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [SubTotal] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Nota] NVARCHAR(500) NULL
);

CREATE TABLE [dbo].[DemoRequests] (
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_DemoReq_Id DEFAULT NEWID() PRIMARY KEY,
    [CustomerName] NVARCHAR(150) NOT NULL,
    [CustomerPhone] NVARCHAR(20) NULL,
    [CustomerEmail] NVARCHAR(100) NULL,
    [CustomerId] UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [dbo].[Clientes]([Id]),
    [TipoServicioId] UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [dbo].[TiposServicio]([Id]),
    [IpAddress] NVARCHAR(50) NULL,
    [Country] NVARCHAR(100) NULL,
    [PhoneVerificationCode] NVARCHAR(6) NULL,
    [IsPhoneVerified] BIT NOT NULL DEFAULT 0,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',  -- Pending | Approved | Rejected | Expired
    [DemoUrl] NVARCHAR(500) NULL,
    [ResponseHtml] NVARCHAR(MAX) NULL,
    [ApprovedBy] UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [dbo].[Usuarios]([Id]),
    [ApprovedAt] DATETIME2 NULL,
    [ExpiresAt] DATETIME2 NULL,
    [RejectionReason] NVARCHAR(255) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE()
);

CREATE NONCLUSTERED INDEX [IX_DemoReq_Status] ON [dbo].[DemoRequests] ([Status]);
CREATE NONCLUSTERED INDEX [IX_DemoReq_IpAddress] ON [dbo].[DemoRequests] ([IpAddress]);

CREATE TABLE [dbo].[Configuracion] (
    [Id] INT NOT NULL PRIMARY KEY DEFAULT 1 CHECK (Id = 1),
    [NombreNegocio] NVARCHAR(200) NOT NULL DEFAULT 'MaxPlus IPTV',
    [Telefono] NVARCHAR(50) NULL,
    [Descripcion] NVARCHAR(500) NULL,
    [Direccion] NVARCHAR(255) NULL,
    [LogoUrl] NVARCHAR(500) NULL,
    [TasaCambioUSD] DECIMAL(18,4) NOT NULL DEFAULT 36.83,
    [DemoPhpBaseUrl] NVARCHAR(500) NULL,
    [MenuPublicoHabilitado] BIT NOT NULL DEFAULT 1,
    [DemoAutoApprove] BIT NOT NULL DEFAULT 0,
    [UpdatedAt] DATETIME2 NULL
);

-- =========================================================================================
-- SEED DATA
-- =========================================================================================
INSERT INTO [dbo].[Roles] ([Nombre], [Descripcion]) VALUES ('Admin', 'Administrador del sistema');
INSERT INTO [dbo].[TiposServicio] ([Nombre], [Descripcion], [Precio], [PrecioCompra], [DurationDays], [Category]) 
VALUES ('Suscripcion Premium 1 Mes', 'Acceso completo a IPTV por 1 mes', 10.00, 3.00, 30, 'Paid');
INSERT INTO [dbo].[TiposServicio] ([Nombre], [Descripcion], [Precio], [PrecioCompra], [DurationDays], [Category]) 
VALUES ('Demo 24 Horas', 'Demo gratuita de IPTV por 24 horas', 0.00, 0.00, 1, 'Demo');
INSERT INTO [dbo].[Configuracion] ([Id], [NombreNegocio], [TasaCambioUSD]) VALUES (1, 'MaxPlus IPTV', 36.83);

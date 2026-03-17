using System.Reflection;
using System.Text;
using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Application.Services;
using MaxPlus.IPTV.Core.Interfaces;
using MaxPlus.IPTV.Infrastructure.Data;
using MaxPlus.IPTV.Infrastructure.Services;
using MaxPlus.IPTV.WebAPI.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ── Swagger con soporte JWT ──────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Ingrese el token JWT. Ejemplo: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ── JWT Authentication ────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew                = TimeSpan.Zero
        };
    });

// ── Rate Limiting (.NET 8 Native) ───────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Política para el Login / Setup (Más restrictiva)
    options.AddFixedWindowLimiter("fixed-auth", opt =>
    {
        opt.Window   = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10; // 10 peticiones por minuto
        opt.QueueLimit  = 0;
    });

    // Política para uso general de la API
    options.AddFixedWindowLimiter("fixed-general", opt =>
    {
        opt.Window   = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100; // 100 peticiones por minuto
        opt.QueueLimit  = 2;
    });
});

builder.Services.AddAuthorization();

// ── Infrastructure ────────────────────────────────────────────────────────────
builder.Services.AddSingleton<DbConnectionFactory>();

// Repositories
builder.Services.AddScoped<IInvoiceRepository,  InvoiceRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IReportRepository,   ReportRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<IUserRepository,     UserRepository>();
builder.Services.AddScoped<IRoleRepository,     RoleRepository>();
builder.Services.AddScoped<ICustomerSubscriptionRepository, CustomerSubscriptionRepository>();
builder.Services.AddScoped<IDemoRequestRepository, DemoRequestRepository>();
builder.Services.AddScoped<IServiceTypeRepository,      ServiceTypeRepository>();
builder.Services.AddScoped<ICategoriaRepository,        CategoriaRepository>();
builder.Services.AddScoped<IPlataformaConfigRepository, PlataformaConfigRepository>();
builder.Services.AddScoped<IOrderRepository,         OrderRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<ISystemLogRepository,     SystemLogRepository>();
builder.Services.AddScoped<IIptvAccountRepository,   IptvAccountRepository>();
builder.Services.AddScoped<IDashboardRepository,     DashboardRepository>();

// External services
builder.Services.AddScoped<IStorageService, CloudinaryStorageService>();
builder.Services.AddHttpClient<IIptvPanelClientService, IptvPanelClientService>();
builder.Services.AddSingleton<IWhatsAppService, GreenApiWhatsAppService>();

// ── Email & Daily Report ──────────────────────────────────────────────────────
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddScoped<IDailyReportJob, DailyReportJob>();
builder.Services.AddHostedService<DailyReportBackgroundService>();
builder.Services.AddHostedService<SubscriptionJobBackgroundService>();

// ── Application ───────────────────────────────────────────────────────────────

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IReportService,   ReportService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IAuthService,     AuthService>();
builder.Services.AddScoped<IUserService,     UserService>();
builder.Services.AddScoped<IRoleService,     RoleService>();

builder.Services.AddScoped<IPdfTicketService, PdfTicketService>();
builder.Services.AddScoped<ICustomerSubscriptionService, CustomerSubscriptionService>();
builder.Services.AddScoped<IDemoService, DemoService>();
builder.Services.AddScoped<IServiceTypeService,        ServiceTypeService>();
builder.Services.AddScoped<ICategoriaService,          CategoriaService>();
builder.Services.AddScoped<IPlataformaConfigService,   PlataformaConfigService>();
builder.Services.AddScoped<IOrderService,         OrderService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<ISubscriptionJobService, SubscriptionJobService>();
builder.Services.AddScoped<IIptvAccountService,     IptvAccountService>();
builder.Services.AddScoped<ICustomerPortalService,  CustomerPortalService>();
builder.Services.AddScoped<IDashboardService,       DashboardService>();

// ── Pipeline ──────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<MaxPlus.IPTV.WebAPI.Middleware.GlobalExceptionMiddleware>();
app.UseRateLimiter(); // ← después de excepción, antes de HTTPS/CORS
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();   // ← antes de UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();

# MaxPlus IPTV - Backend

API REST para la plataforma MaxPlus IPTV, construida con .NET 8, Dapper y SQL Server.

## Stack Tecnológico

| Capa | Tecnología |
|------|-----------|
| API | ASP.NET Core 8 Web API |
| ORM | Dapper (micro-ORM) |
| DB | SQL Server (Stored Procedures) |
| Auth | JWT Bearer Tokens + BCrypt |
| PDFs | QuestPDF + SkiaSharp |
| Cloud | Cloudinary (estados de cuenta) |

## Estructura del Proyecto

```
src/
├── MaxPlus.IPTV.WebAPI           → Controladores REST, middlewares, DI
├── MaxPlus.IPTV.Application      → Lógica de negocio, servicios, DTOs, interfaces
├── MaxPlus.IPTV.Infrastructure   → Repositorios (Dapper), servicios externos
└── MaxPlus.IPTV.Core             → Entidades del dominio
```

## Configuración

Crea/edita `appsettings.json` en `src/MaxPlus.IPTV.WebAPI/`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=MaxPlusIPTV;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "TU_CLAVE_SECRETA_DE_32_CARACTERES_MINIMO",
    "Issuer": "MaxPlusIPTV",
    "Audience": "MaxPlusIPTV_Users"
  }
}
```

## Ejecución

```bash
dotnet run --project src/MaxPlus.IPTV.WebAPI/
```

## Base de Datos

Ejecutar los scripts en orden en una BD limpia:

1. `Database/01_IPTV_Tables.sql` — Tablas con `UNIQUEIDENTIFIER`
2. `Database/02_IPTV_StoredProcedures.sql` — Stored Procedures

## Funcionalidades

- **Autenticación**: Login/Setup con JWT
- **Clientes**: CRUD con búsqueda
- **Facturación**: Creación de facturas con detalles, soporte crédito/contado
- **Tipos de Servicio**: Catálogo configurable (ej: Suscripción Premium)
- **Cuentas por Cobrar**: Registro de abonos con distribución FIFO
- **Estados de Cuenta**: Generación PDF + almacenamiento en Cloudinary
- **Reportes**: Resumen de ventas, historial, top productos
- **Configuración**: Ajustes globales del negocio

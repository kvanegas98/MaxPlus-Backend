# MaxPlus IPTV — Documentación Técnica

## Descripción general

Sistema de gestión de suscripciones IPTV con panel administrativo. Permite gestionar clientes, cuentas IPTV, suscripciones, facturación, demos y notificaciones por WhatsApp y correo electrónico.

---

## Arquitectura

Implementa **Clean Architecture** en capas:

```
MaxPlus.IPTV.WebAPI          → Controllers, Background Services, Program.cs
MaxPlus.IPTV.Application     → Services, DTOs, Interfaces de servicios
MaxPlus.IPTV.Core            → Entities, Interfaces de repositorios
MaxPlus.IPTV.Infrastructure  → Repositorios (Dapper), Servicios externos
```

**Stack tecnológico:**
- .NET 8 Web API
- SQL Server + Dapper (Stored Procedures para todo acceso a datos)
- JWT Bearer Authentication
- QuestPDF (generación de facturas)
- PDFtoImage + SkiaSharp (conversión PDF → PNG)
- Cloudinary (almacenamiento de imágenes)
- Twilio (WhatsApp)
- SMTP (email de respaldo)

---

## Endpoints

### Autenticación
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/auth/login` | Login con email + password. Retorna JWT. |
| POST | `/api/auth/setup` | Crea el primer usuario admin (solo si no existe ninguno). |

### Clientes
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/customers` | Lista todos los clientes. |
| GET | `/api/customers/{id}` | Obtiene un cliente por ID. |
| GET | `/api/customers/search` | Busca cliente por nombre o teléfono. |
| POST | `/api/customers` | Crea un cliente. |
| PUT | `/api/customers/{id}` | Actualiza un cliente. |
| DELETE | `/api/customers/{id}` | Elimina un cliente. |

### Cuentas IPTV
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/iptv-accounts/with-clients` | Lista cuentas con sus slots y clientes asignados. |
| GET | `/api/iptv-accounts/stats` | Totales de cuentas, slots usados y disponibles. |
| GET | `/api/iptv-accounts/by-service/{tipoServicioId}` | Cuentas con slots disponibles para un tipo de servicio. |
| GET | `/api/iptv-accounts` | Lista todas las cuentas. |
| GET | `/api/iptv-accounts/{id}` | Obtiene una cuenta por ID. |
| POST | `/api/iptv-accounts` | Crea una cuenta IPTV. |
| PUT | `/api/iptv-accounts/{id}` | Actualiza una cuenta. |
| DELETE | `/api/iptv-accounts/{id}` | Desactiva una cuenta. |
| POST | `/api/iptv-accounts/{id}/assign` | Asigna un cliente a un slot. Genera factura y envía WhatsApp + email. |

### Suscripciones
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/subscriptions/active` | Lista suscripciones activas. |
| GET | `/api/subscriptions/unassigned` | Lista suscripciones sin cliente. |
| GET | `/api/subscriptions/customer/{customerId}` | Suscripciones de un cliente. |
| POST | `/api/subscriptions` | Crea una suscripción directa. |
| PUT | `/api/subscriptions/{id}` | Actualiza una suscripción. |
| DELETE | `/api/subscriptions/{id}` | Cancela y libera el slot. |
| POST | `/api/subscriptions/{id}/assign` | Asigna cliente a suscripción existente. |
| POST | `/api/subscriptions/{id}/renew` | Renueva suscripción. Genera factura y envía WhatsApp + email con imagen adjunta. |

### Órdenes web
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/orders` | Crea una orden web (público, sin auth). |
| GET | `/api/orders` | Lista órdenes web. |
| GET | `/api/orders/{id}` | Obtiene una orden por ID. |
| POST | `/api/orders/{id}/approve` | Aprueba orden, asigna slot, genera factura y envía WhatsApp. |
| POST | `/api/orders/{id}/reject` | Rechaza una orden web. |

### Demos
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/demo/request` | Solicita una demo (público). |
| POST | `/api/demo/{id}/verify` | Verifica token de demo. |
| GET | `/api/demo` | Lista demos. |
| GET | `/api/demo/{id}` | Obtiene una demo. |
| POST | `/api/demo/{id}/approve` | Aprueba demo y envía credenciales por WhatsApp. |
| POST | `/api/demo/{id}/reject` | Rechaza una demo. |

### Facturación / Reportes
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/reports/summary` | Resumen de ventas del día. |
| GET | `/api/reports/sales-by-hour` | Ventas por hora. |
| GET | `/api/reports/top-products` | Productos más vendidos. |
| GET | `/api/reports/order-history` | Historial de órdenes/facturas. |
| POST | `/api/reports/enviar-reporte-diario` | Envía reporte diario por email. |

### Dashboard
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/dashboard/summary` | Resumen financiero y operativo. Parámetros opcionales: `fechaDesde`, `fechaHasta`. |
| GET | `/api/dashboard/expiring` | Suscripciones por vencer. Parámetro opcional: `days` (default 30). |

### Portal del cliente (público)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/portal/lookup?customerId={guid}` | Retorna suscripciones, demos y facturas del cliente. No requiere autenticación. |

### Tipos de servicio
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/service-types` | Lista tipos de servicio. |
| GET | `/api/service-types/{id}` | Obtiene un tipo de servicio. |
| POST | `/api/service-types` | Crea tipo de servicio. |
| PUT | `/api/service-types/{id}` | Actualiza tipo de servicio. |
| DELETE | `/api/service-types/{id}` | Elimina tipo de servicio. |
| POST | `/api/service-types/upload-image` | Sube imagen del servicio. |
| GET | `/api/service-types/plataformas-config` | Configuración de plataformas. |

### Métodos de pago
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/payment-methods` | Lista métodos de pago con datos bancarios. |
| POST | `/api/payment-methods` | Crea método de pago. |
| PUT | `/api/payment-methods/{id}` | Actualiza. |
| DELETE | `/api/payment-methods/{id}` | Elimina. |

### Configuración
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/settings` | Configuración del sistema (requiere auth). |
| GET | `/api/settings/public` | Configuración pública (nombre del negocio, etc.). |
| PUT | `/api/settings` | Actualiza configuración. |

### Otros
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/logs` | Logs del sistema. |
| GET | `/api/roles` | Lista roles. |
| GET | `/api/users` | Lista usuarios. |
| POST | `/api/users` | Crea usuario. |
| PUT | `/api/users/{id}` | Actualiza usuario. |
| POST | `/api/users/{id}/change-password` | Cambia contraseña. |

---

## Flujos principales

### Flujo de aprobación de orden web
1. Cliente llena formulario en el frontend → `POST /api/orders`
2. Admin ve la orden en el panel → `GET /api/orders`
3. Admin selecciona cuenta IPTV disponible → `GET /api/iptv-accounts/by-service/{tipoServicioId}`
4. Admin aprueba → `POST /api/orders/{id}/approve`
5. Sistema: asigna slot, genera factura, sube PNG a Cloudinary, envía WhatsApp con credenciales + imagen

### Flujo de asignación directa (sin orden web)
1. Admin crea/selecciona cliente
2. Admin selecciona cuenta IPTV → `POST /api/iptv-accounts/{id}/assign`
3. Sistema: asigna slot, genera factura, envía WhatsApp + email

### Flujo de renovación
1. Admin selecciona suscripción activa o vencida → `POST /api/subscriptions/{id}/renew`
2. Sistema: marca anterior como `Renewed`, crea nueva suscripción, genera factura, envía WhatsApp con imagen + email

### Flujo de demo
1. Cliente solicita demo → `POST /api/demo/request`
2. Admin aprueba → `POST /api/demo/{id}/approve`
3. Sistema envía credenciales de prueba por WhatsApp (válidas 4 horas)

### Job automático (diario 9:00 AM Nicaragua)
- Notifica por WhatsApp a clientes con suscripción que vence en 7 días
- Notifica por WhatsApp a clientes con suscripción que vence en 1 día
- Marca como `Expired` todas las suscripciones vencidas

---

## Credenciales por tipo de plataforma

| Plataforma | Credenciales enviadas |
|---|---|
| IPTV / FlujoTV | Usuario + Contraseña + PIN (si aplica) |
| Netflix / Streaming | Email de cuenta + Contraseña + Usuario de perfil + PIN de perfil |

La configuración de qué campos mostrar se gestiona en `PlataformasConfig` (tabla `PlataformasConfig` en BD).

---

## Configuración (appsettings.json)

```json
{
  "ConnectionStrings": { "DefaultConnection": "..." },
  "Jwt": { "Key": "", "Issuer": "MaxPlusIPTV", "Audience": "MaxPlusIPTV" },
  "Twilio": { "AccountSid": "", "AuthToken": "", "FromNumber": "whatsapp:+1..." },
  "Cloudinary": { "CloudName": "", "ApiKey": "", "ApiSecret": "" },
  "EmailSettings": {
    "Host": "", "Port": 587, "UseSsl": true,
    "Username": "", "Password": "",
    "FromName": "MaxPlus IPTV", "FromEmail": "",
    "ReportRecipients": ["admin@correo.com"]
  }
}
```

---

## Scripts SQL (orden de ejecución inicial)

| Archivo | Propósito |
|---|---|
| `IptvAccounts.sql` | Tablas principales, SPs base |
| `IptvAccounts_TipoServicio.sql` | Agrega TipoServicioId a IptvAccounts |
| `IptvAccounts_AccessEmail.sql` | Agrega AccessEmail a IptvAccounts |
| `IptvAccounts_PorTipoServicio.sql` | SP para filtrar cuentas por servicio |
| `CustomerSubscriptions_ProfileCredentials.sql` | ProfileUser + ProfilePin en slots |
| `ServiceOrders.sql` | Tabla de órdenes web |
| `ServiceOrders_NumeroOrden.sql` | Consecutivo WEB-000001 |
| `Facturas_AddServiceOrderId.sql` | Vincula factura con orden web |
| `Facturas_MetodoPagoJoin.sql` | Datos bancarios en factura |
| `ServiceType_Plataforma.sql` | Campo Plataforma en TiposServicio |
| `PlataformasConfig.sql` | Tabla de configuración por plataforma |
| `SubscriptionJobs.sql` | Tabla SystemLogs + columnas de notificación |
| `SubscriptionJobs_SoloSPs.sql` | SPs del job automático |
| `Dashboard_Resumen.sql` | SPs del dashboard |
| `CustomerPortal_SPs.sql` | SPs del portal del cliente |

---

## Estructura de base de datos (tablas principales)

| Tabla | Descripción |
|---|---|
| `Clientes` | Clientes del negocio |
| `IptvAccounts` | Cuentas IPTV compradas al proveedor |
| `CustomerSubscriptions` | Slots asignados (un slot = un cliente en una cuenta) |
| `TiposServicio` | Catálogo de servicios (Netflix, IPTV, etc.) |
| `ServiceOrders` | Órdenes web de clientes |
| `Facturas` | Facturas generadas |
| `InvoiceDetails` | Líneas de cada factura |
| `MetodosPago` | Métodos de pago con datos bancarios |
| `PlataformasConfig` | Configuración de credenciales por plataforma |
| `DemoRequests` | Solicitudes de demo |
| `SystemLogs` | Logs del job automático |
| `Users` | Usuarios del sistema |
| `Roles` | Roles (Admin, etc.) |
| `Settings` | Configuración general del negocio |

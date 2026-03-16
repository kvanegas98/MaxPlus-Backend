# MaxPlus IPTV API - Endpoints Documentation

Este documento detalla los Endpoints REST de la API de MaxPlus IPTV para consumo del Frontend.

> Base URL esperada (local): `https://localhost:7199/api` o `http://localhost:5253/api` (ver terminal)
> Autenticación: Todos los endpoints requieren el header `Authorization: Bearer <token>` **EXCEPTO** los marcados como [Público].

---

## 🔐 1. Autenticación (`/api/Auth`)
Controla el acceso al sistema emitiendo Tokens JWT.

- **`POST /api/Auth/login`** [Público]
  - **Uso:** Iniciar sesión.
  - **Body (JSON):** `{ "email": "admin@k.com", "password": "123" }`
  - **Response (200 OK):** `{ "token": "...", "expiration": "2026-03-05T20:00:00Z" }`

- **`POST /api/Auth/setup`** [Público]
  - **Uso:** Crea el *primer* usuario Administrador (solo funciona si la tabla Usuarios está vacía).
  - **Body:** `{ "fullName": "Admin", "email": "...", "password": "..." }`

---

## ⚙️ 2. Tipos de Servicio / Catálogo (`/api/ServiceTypes`)
Los planes disponibles (Ej: "Plan Premium 1 Mes").

- **`GET /api/ServiceTypes`**
  - **Uso:** Lista todos los planes activos.
- **`GET /api/ServiceTypes/{id}`**
  - **Uso:** Detalle de un plan específico.
- **`POST /api/ServiceTypes`** [Solo Admin]
  - **Uso:** Crea un nuevo plan en el catálogo.
  - **Body:** `{ "name": "Premium 1 Mes", "description": "...", "price": 10.00, "purchasePrice": 3.00, "durationDays": 30, "category": "Paid", "imageUrl": null }`
- **`PUT /api/ServiceTypes/{id}`** [Solo Admin]
  - **Uso:** Actualiza los precios, duraciones o detalles.
- **`DELETE /api/ServiceTypes/{id}`** [Solo Admin]
  - **Uso:** Soft-delete (desactiva) el plan.

---

## 📡 3. Cuentas Proveedores IPTV (`/api/IptvAccounts`)
Paneles mayoristas de donde sacas las conexiones para tus clientes.

- **`GET /api/IptvAccounts`** [Solo Admin]
- **`POST /api/IptvAccounts`** [Solo Admin]
  - **Body:** `{ "username": "...", "password": "...", "hostUrl": "http://..", "maxConnections": 3, "expirationDate": "2026-10-10", "nota": "" }`
- **`PUT /api/IptvAccounts/{id}`** & **`DELETE /api/IptvAccounts/{id}`** [Solo Admin]

---

## 👥 4. Clientes (`/api/Customers`)
Directorio de clientes finales.

- **`GET /api/Customers`**
- **`GET /api/Customers/search?term={termino}`** (Buscar por nombre/teléfono)
- **`POST /api/Customers`**
  - **Body:** `{ "nombre": "Juan", "telefono": "300...", "direccion": "...", "email": "" }`
- **`PUT /api/Customers/{id}`** & **`DELETE /api/Customers/{id}`**

---

## 🎟️ 5. Suscripciones (`/api/Subscriptions`)
Relaciona a un `Customer` con un `ServiceType` y una `IptvAccount`.

- **`GET /api/Subscriptions`** (Historial completo)
- **`GET /api/Subscriptions/active`** (Suscripciones activas actuales)
- **`GET /api/Subscriptions/customer/{customerId}`** (Suscripciones de un cliente)
- **`POST /api/Subscriptions`**
  - **Body:** 
    ```json
    {
      "customerId": "guid",
      "providerAccountId": "guid", // opcional
      "tipoServicioId": "guid",    // Obligatorio para vincular el plan
      "platformUrl": "http://magnum...",
      "accessUser": "juan123",
      "accessPassword": "123",
      "pinCode": null,
      "expirationDate": "2026-04-05T00:00:00Z"
    }
    ```
- **`PUT /api/Subscriptions/{id}`** (Modificar datos de acceso / url)
- **`PUT /api/Subscriptions/{id}/cancel`** (Dar de baja anticipadamente)

---

## 🆓 6. Demos / Pruebas (`/api/Demo`)

- **`POST /api/Demo/request`** [Público] (Cliente pide Demo)
  - **Body:** `{"customerName": "Juan", "customerPhone": "...", "customerEmail": "...", "tipoServicioId": "guid"}`
- **`GET /api/Demo/pending`** (Bandeja de entrada para el Admin)
- **`PUT /api/Demo/{id}/approve`** (Admin aprueba inyectando URL + Credenciales)
- **`PUT /api/Demo/{id}/reject`** (Admin rechaza)

---

## 🛠️ 7. Configuración (`/api/Settings`)

- **`GET /api/Settings`** [Público en parte] (Devuelve LogoUrl, NombreNegocio, MenuPublicoHabilitado)
- **`PUT /api/Settings`** [Solo Admin] (Guardar nueva configuración)

---

## 👤 8. Empleados y Roles (`/api/Users` & `/api/Roles`)

- **`GET /api/Users`** [Solo Admin]
- **`POST /api/Users`** [Solo Admin] (Crea un nuevo empleado)
- **`PUT /api/Users/{id}`** [Solo Admin]
- **`PUT /api/Users/{id}/password`** (Cambiar clave propia)
- **`PUT /api/Users/{id}/deactivate`** [Solo Admin]
- **`GET /api/Roles`** [Solo Admin] (Obtiene Admin, Cajero, Vendedor, etc)

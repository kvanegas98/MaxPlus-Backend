# MaxPlus IPTV - Futuras Mejoras (Fase 3+)

Este documento lista las ideas y funcionalidades avanzadas que pueden ser implementadas en el backend de **MaxPlus IPTV** para automatizar la operación del negocio y reducir la carga manual del administrador.

---

## 🕒 1. Cortes y Suspensiones Automáticas
**Objetivo:** Evitar tener que revisar manualmente quién ya no ha pagado su mensualidad.
- **Implementación (Backend puro):** 
  - Crear un *Worker Service* (Background Job en .NET) que se ejecute todos los días a las 12:00 AM.
  - El sistema buscará todas las suscripciones en la tabla `CustomerSubscriptions` cuya fecha `ExpirationDate` sea menor al día de hoy y que sigan en estado `Active`.
  - El sistema cambiará automáticamente el estado a `Expired`.
- **Costo:** $0 (Todo es lógica interna).

## 🔌 2. Conexión Directa con el Panel IPTV (Auto-Activación)
**Objetivo:** Que al registrar/aprobar un cliente en nuestro sistema, se le cree automáticamente su usuario y contraseña en el panel del proveedor mayorista (Magnum, Mega, Xtream UI, etc.).
- **Implementación (Integración de APIs):**
  - Utilizar la **API de Revendedor (Reseller API)** que los paneles IPTV ya incluyen de forma gratuita.
  - Cuando se hace un `POST /api/Subscriptions` (Nueva venta) o se aprueba un Demo, nuestro backend se conectará en milisegundos a la URL del proveedor (`IptvProviderAccounts.HostUrl`) y ejecutará el comando para generar las credenciales.
  - Al recibir las credenciales del panel, las guardamos en nuestra base de datos (`AccessUser`, `AccessPassword`) y se las mostramos al cliente.
  - Del mismo modo, si la suscripción de un cliente expira (punto 1), el backend le manda el comando al panel para "Desactivar/Banear" la línea IPTV.
- **Costo:** $0 (Se usan los mismos créditos que ya compras al mayorista).

## 📱 3. Notificaciones Automáticas (Recordatorios de Pago)
**Objetivo:** Aumentar la tasa de renovación enviando mensajes automáticos a los clientes cuando su servicio esté a punto de vencer.
- **Implementación (WhatsApp / Email):**
  - El mismo *Worker Service* buscará clientes cuya fecha de vencimiento sea en exactamente **3 días** o **1 día**.
  - **Emails (Gratis):** Se usa el servicio `IEmailService` ya existente en la API para despachar un correo formateado recordando que el servicio vence pronto.
  - **WhatsApp (Opción A - Gratis / No Oficial):** Desplegar una pequeña API local (ej: `whatsapp-web.js` en Node.js) que se vincula a un celular escaneando un código QR. Nuestro backend en .NET le mandará una petición a este servicio para que dispare el WhatsApp al cliente simulando que lo enviaste tú.
  - **WhatsApp (Opción B - API Oficial Cloud):** Usar Meta/Twilio. Es 100% estable pero tiene un costo aproximado de $0.01 USD por mensaje.
- **Costo:** Desde $0 (usando Email o WhatsApp Web) hasta unos pocos dólares si se usa la API oficial de Meta.

## 📊 4. Portal de Auto-Gestión del Cliente
**Objetivo:** Que el cliente no tenga que escribirte por WhatsApp para ver sus credenciales, cuándo se le vence el servicio, o para renovar.
- **Implementación (FrontEnd + API):**
  - Crear un Portal de Cliente donde este inicie sesión con su Email o Teléfono.
  - El cliente solo podrá ver su historial de facturas, sus servicios activos, cuántos días le quedan, y un botón para contactarte por WhatsApp para renovar.

## 💳 5. Pasarelas de Pago Cripto / Automáticas
**Objetivo:** Permitir que los clientes paguen su renovación a las 3:00 AM sin tu intervención y se les reactive el servicio al instante.
- **Implementación (Integración Externa):**
  - Conectar pasarelas como **PayPal**, **Stripe**, **MercadoPago** (según el país), o pasarelas de criptomonedas como **Binance Pay / Coinbase Commerce**.
  - Al recibir el Webhook de "Pago Exitoso", nuestro backend automáticamente renueva la `ExpirationDate` por sumándole los `DurationDays` del plan pagado.
  - **Costo:** Las pasarelas cobran un % de comisión por transacción exitosa (ej. 2.9% + $0.30 en PayPal). El desarrollo interno es lógica pura en .NET.

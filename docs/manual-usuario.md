# MaxPlus IPTV — Manual de Usuario

## ¿Qué es MaxPlus?

MaxPlus es el panel de administración para gestionar tu negocio de suscripciones IPTV. Desde aquí puedes agregar clientes, asignar servicios, cobrar, renovar y enviar credenciales automáticamente por WhatsApp.

---

## Acceso al sistema

1. Abre el panel en tu navegador
2. Ingresa tu correo electrónico y contraseña
3. El sistema te dará acceso según tu rol (Administrador)

---

## Módulos principales

### 1. Clientes

Lista de todos tus clientes con nombre, teléfono, correo y fecha de registro.

**Acciones disponibles:**
- **Nuevo cliente** — Registra nombre, teléfono y correo
- **Editar** — Actualiza los datos del cliente
- **Ver detalle** — Ver suscripciones, demos y facturas del cliente
- **Eliminar** — Solo si el cliente no tiene suscripciones activas

---

### 2. Cuentas IPTV

Las cuentas que compraste al proveedor. Cada cuenta tiene un número máximo de slots (espacios para clientes).

**¿Qué ves en esta pantalla?**
- Credenciales de la cuenta (usuario, contraseña, email)
- Tipo de servicio (Netflix, IPTV, FlujoTV, etc.)
- Slots ocupados y disponibles
- Fecha de vencimiento de la cuenta
- Quién está en cada slot

**Acciones disponibles:**
- **Nueva cuenta** — Registra las credenciales que te dio el proveedor
- **Editar** — Actualiza credenciales o fecha de vencimiento
- **Asignar cliente** — Ocupa un slot con un cliente
- **Liberar slot** — Cancela la suscripción de un cliente y deja el espacio libre

---

### 3. Asignar un cliente a una cuenta (flujo normal)

1. Ve a **Cuentas IPTV**
2. Selecciona la cuenta que tiene slots disponibles
3. Haz clic en **Asignar cliente**
4. Rellena:
   - Cliente (busca por nombre o teléfono, o crea uno nuevo)
   - Fecha de vencimiento (o se calcula automáticamente según el servicio)
   - Para Netflix/Streaming: usuario de perfil y PIN de perfil del slot
   - Método de pago, referencia de pago y monto recibido
5. Haz clic en **Guardar**

El sistema automáticamente:
- Crea la suscripción
- Genera la factura
- Envía las credenciales al cliente por **WhatsApp** con la factura adjunta
- Envía un correo de respaldo si el cliente tiene email registrado

---

### 4. Órdenes web

Cuando un cliente llena el formulario de solicitud en tu sitio web, aparece aquí como una orden pendiente.

**Estados de una orden:**
- **Pendiente** — Recién llegada, esperando revisión
- **Aprobada** — Ya fue procesada y el cliente tiene su servicio
- **Rechazada** — Fue denegada

**Para aprobar una orden:**
1. Ve a **Órdenes web**
2. Haz clic en la orden pendiente
3. Selecciona la cuenta IPTV donde vas a asignar al cliente
4. Para Netflix/Streaming ingresa el usuario y PIN de perfil
5. Completa los datos de pago (método, referencia, monto)
6. Haz clic en **Aprobar**

El número de orden tiene formato **WEB-000001** y aparece en la factura.

---

### 5. Renovar una suscripción

1. Ve a **Suscripciones** o busca al cliente
2. Selecciona la suscripción que quieres renovar
3. Haz clic en **Renovar**
4. Ingresa la nueva fecha de vencimiento
5. Completa los datos de pago
6. Confirma

El sistema:
- Crea una nueva suscripción con las mismas credenciales
- Marca la anterior como renovada
- Genera nueva factura
- Envía confirmación por WhatsApp con la factura adjunta

---

### 6. Liberar un slot (cliente no pagó)

Si un cliente no renovó y quieres dar ese espacio a otro:

1. Ve a **Cuentas IPTV**
2. Busca la cuenta con ese cliente
3. En el slot ocupado haz clic en **Liberar slot**
4. Confirma la acción

El espacio queda disponible inmediatamente para un nuevo cliente. El historial del cliente anterior se conserva.

---

### 7. Demos

Cuando alguien solicita una prueba gratuita desde tu sitio web, aparece aquí.

**Para aprobar una demo:**
1. Ve a **Demos**
2. Selecciona la demo pendiente
3. Haz clic en **Aprobar**
4. El sistema envía credenciales de prueba al cliente por WhatsApp (válidas 4 horas)

---

### 8. Facturación

Cada vez que asignas un cliente, apruebas una orden o renuevas, se genera una factura automáticamente.

**La factura incluye:**
- Número de orden (ej: WEB-000042)
- Nombre del cliente
- Concepto del servicio
- Método de pago (banco, tipo de cuenta, número de cuenta, titular)
- Referencia de transferencia
- Monto total
- Fecha

La factura se envía como imagen PNG directamente al WhatsApp del cliente.

---

### 9. Dashboard

Vista rápida del estado de tu negocio:

- **Ingresos del mes** — Total facturado en el período
- **Costos** — Lo que pagaste por cuentas IPTV
- **Ganancia neta** — Ingresos menos costos
- **Suscripciones activas** — Clientes con servicio vigente
- **Por vencer** — Cuántos clientes vencen en 7 y 30 días
- **Gráfica mensual** — Ingresos de los últimos 6 meses

Puedes filtrar por rango de fechas para ver períodos anteriores.

---

### 10. Notificaciones automáticas

El sistema envía notificaciones automáticas **todos los días a las 9:00 AM** sin que tengas que hacer nada:

| Cuándo | Qué hace |
|---|---|
| 7 días antes del vencimiento | WhatsApp al cliente recordando que su servicio vence pronto |
| 1 día antes del vencimiento | WhatsApp final de aviso de vencimiento |
| Al vencerse | La suscripción se marca automáticamente como vencida |

---

### 11. Portal del cliente

Tus clientes pueden consultar su información en el portal público ingresando su ID de cliente.

Desde el portal el cliente puede ver:
- Sus suscripciones activas con credenciales
- Sus demos solicitadas
- Su historial de facturas

El ID del cliente lo maneja el administrador desde el panel de clientes.

---

## Tipos de servicio y plataformas

Cada servicio tiene configurada su plataforma. Esto afecta qué credenciales se envían al cliente:

| Plataforma | Credenciales enviadas por WhatsApp |
|---|---|
| IPTV | Usuario + Contraseña + PIN (si aplica) |
| FlujoTV | Usuario + Contraseña |
| Netflix | Email de la cuenta + Contraseña + Usuario de perfil + PIN de perfil |
| Streaming | Email de la cuenta + Contraseña + Usuario de perfil + PIN de perfil |

---

## Métodos de pago

Registra tus cuentas bancarias en **Métodos de pago**. Al generar una factura puedes seleccionar el método con el que el cliente pagó, y esos datos aparecerán en la imagen de la factura que se envía por WhatsApp.

---

## Preguntas frecuentes

**¿Por qué no le llegó el WhatsApp al cliente?**
El cliente debe tener un número de teléfono registrado. En modo sandbox de Twilio, el cliente también debe haber enviado el mensaje `join <código>` al número de WhatsApp del sistema. En producción con número aprobado esto no es necesario.

**¿Puedo renovar una suscripción vencida?**
Sí. Puedes renovar tanto suscripciones activas como vencidas.

**¿Qué pasa si libero un slot por error?**
Debes volver a asignar al cliente manualmente. La operación no se puede deshacer automáticamente, pero el cliente no se elimina del sistema.

**¿Las credenciales cambian al renovar?**
No. La nueva suscripción hereda exactamente las mismas credenciales de acceso. Solo cambia la fecha de vencimiento.

**¿Dónde veo los errores del sistema?**
En el módulo **Logs** puedes ver todas las operaciones del job automático, incluyendo notificaciones enviadas y errores.

# Implementación de Demos Automáticos IPTV (Xtream Codes API)

Este documento detalla el plan para habilitar la auto-gestión de Demos (Pruebas gratuitas) con conexión directa al servidor mayorista IPTV y reglas estrictas de anti-abuso.

## Archivo de configuración
- [ ] Asegurar que `appsettings.json` o la base de datos almacene correctamente el Endpoint Master del proveedor.
- Actualmente, la url master del panel se almacena en la tabla `IptvProviderAccounts` (`HostUrl`, `Username`, `Password`).

## Fases de Desarrollo

### 1. Seguridad Anti-Abuso (Base de Datos & Capa de Servicio)
- [ ] Modificar `DemoRequest.cs` (Entidad) y `01_IPTV_Tables.sql` para añadir:
  - `IpAddress` (NVARCHAR) para registrar desde dónde se pidió el demo.
  - `Country` (Opcional, basado en IP).
  - `PhoneVerificationCode` (NVARCHAR) para almacenar el código (OTP) enviado por WhatsApp.
  - `IsPhoneVerified` (BIT).
- [ ] Crear Service para chequear si la IP o el Teléfono ya pidieron un Demo en las últimas 48 horas / 30 días.

### 2. Integración Xtreme Codes API (Capa Infraestructura)
- [ ] Crear un `IptvPanelClientService.cs` que utilizará `HttpClient`.
- [ ] Implementar método genérico `CreateTrialUserAsync(...)`:
  - Enviar HTTP GET a `[HostUrl]/api.php?username=[User]&password=[Pass]&action=create_line_user&username={Rnd}&password={Rnd}&is_trial=1` o similar dependiendo del panel.
  - Decodificar el JSON devuelto por el servidor y extraer el acceso otorgado.

### 3. Actualización de Controladores
- [ ] `POST /api/Demo/request`: Guardará el intento, validará la IP, y si es válido, generará un código OTP y lo despachará al WhatsApp/Email. Devolverá "Requiere validación".
- [ ] `POST /api/Demo/verify`: El cliente envía el OTP desde el Frontend.
  - Si es correcto, el backend se conecta instantáneamente a Xtream API (`IptvPanelClientService`).
  - Obtiene el usuario IPTV, marca el DemoRequest como `Approved`.
  - Devuelve las credenciales de la TV al Frontend.

### 4. Background Service (Limpieza) -> *Opcional futuro* 
- [ ] Borrar los demos pendientes que se quedaron huérfanos sin validar.

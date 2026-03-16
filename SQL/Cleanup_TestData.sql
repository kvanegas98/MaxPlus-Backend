-- ============================================================
-- MaxPlus - Script de limpieza de datos de prueba
-- Conserva: Users, Roles, ServiceTypes, Settings,
--           PaymentMethods, PlataformasConfig
-- Elimina:  Customers, Demos, Subscriptions, Orders, Invoices
-- ============================================================

BEGIN TRANSACTION;

BEGIN TRY

  -- 1. Detalles de facturas
  DELETE FROM InvoiceDetails;

  -- 2. Facturas
  DELETE FROM Facturas;

  -- 3. Órdenes web (no tienen tabla de detalle separada)
  DELETE FROM ServiceOrders;

  -- 5. Demos
  DELETE FROM DemoRequests;

  -- 6. Suscripciones
  DELETE FROM CustomerSubscriptions;

  -- 7. Liberar cuentas IPTV (no borrarlas, solo resetear asignación)
  -- Las cuentas IPTV no tienen CustomerId directo, los slots están en CustomerSubscriptions
  -- Si hay campos de asignación en IptvAccounts, resetear aquí

  -- 8. Clientes
  DELETE FROM Clientes;


   DELETE FROM dbo.IptvAccounts;

  COMMIT TRANSACTION;
  PRINT 'Limpieza completada correctamente.';

END TRY
BEGIN CATCH
  ROLLBACK TRANSACTION;
  PRINT 'Error — se hizo rollback:';
  PRINT ERROR_MESSAGE();
END CATCH;

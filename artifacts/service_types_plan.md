# Implementación del Catálogo Tipos de Servicio (ServiceTypes)

## Objetivo
Implementar el CRUD completo para la tabla `TiposServicio` en la arquitectura del proyecto MaxPlus IPTV. 

## Archivos a Modificar / Crear

### 1. Entidades y DTOs (Capa `MaxPlus.IPTV.Core` y `MaxPlus.IPTV.Application`)
- [ ] `src/MaxPlus.IPTV.Core/Entities/ServiceType.cs` (Modelo de dominio)
- [ ] `src/MaxPlus.IPTV.Application/DTOs/ServiceType/ServiceTypeDto.cs` (Modelo de salida)
- [ ] `src/MaxPlus.IPTV.Application/DTOs/ServiceType/ServiceTypeCreateDto.cs` (Entrada para Create)
- [ ] `src/MaxPlus.IPTV.Application/DTOs/ServiceType/ServiceTypeUpdateDto.cs` (Entrada para Update)

### 2. Interfaces (Capa `MaxPlus.IPTV.Core` y `MaxPlus.IPTV.Application`)
- [ ] `src/MaxPlus.IPTV.Core/Interfaces/IServiceTypeRepository.cs` (Contrato del Repositorio)
- [ ] `src/MaxPlus.IPTV.Application/Interfaces/IServiceTypeService.cs` (Contrato del Business Logic)

### 3. Implementaciones (Capa `MaxPlus.IPTV.Infrastructure` y `MaxPlus.IPTV.Application`)
- [ ] `src/MaxPlus.IPTV.Infrastructure/Data/ServiceTypeRepository.cs` (Dapper calls a `sp_TiposServicio_...`)
- [ ] `src/MaxPlus.IPTV.Application/Services/ServiceTypeService.cs` (Lógica de negocio y validación)

### 4. API (Capa `MaxPlus.IPTV.WebAPI`)
- [ ] `src/MaxPlus.IPTV.WebAPI/Controllers/ServiceTypesController.cs` (Endpoints GET, POST, PUT, DELETE)
- [ ] `src/MaxPlus.IPTV.WebAPI/Program.cs` (Registro de dependencias DI)

## Resumen de Endpoints
- `GET /api/ServiceTypes` (Listar activos)
- `GET /api/ServiceTypes/{id}` (Obtener detalle)
- `POST /api/ServiceTypes` (Crear nuevo)
- `PUT /api/ServiceTypes/{id}` (Actualizar)
- `DELETE /api/ServiceTypes/{id}` (Soft delete vía IsActive=0)

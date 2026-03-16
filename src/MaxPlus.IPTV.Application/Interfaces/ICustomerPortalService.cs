using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface ICustomerPortalService
{
    /// <summary>
    /// Retorna toda la información de un cliente identificado por su ID:
    /// suscripciones activas/historial, demos solicitadas y facturas.
    /// </summary>
    Task<CustomerPortalResponseDto?> GetByCustomerIdAsync(Guid customerId);
}

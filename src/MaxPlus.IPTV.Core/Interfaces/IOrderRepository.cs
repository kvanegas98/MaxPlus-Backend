using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface IOrderRepository
{
    Task<(Guid Id, string NumeroOrden)> AddAsync(ServiceOrder order);
    Task<ServiceOrder?> GetByIdAsync(Guid id);
    Task<IEnumerable<ServiceOrder>> GetAllAsync(string? status = null);
    Task ApproveAsync(Guid id, Guid approvedBy, Guid subscriptionId);
    Task RejectAsync(Guid id, Guid approvedBy, string? reason);

    // Carrito — items
    Task<Guid> AddItemAsync(ServiceOrderItem item);
    Task<IEnumerable<ServiceOrderItem>> GetItemsByOrderIdAsync(Guid orderId);
    Task AssignItemSubscriptionAsync(Guid itemId, Guid subscriptionId);
}

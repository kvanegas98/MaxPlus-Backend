using MaxPlus.IPTV.Core.Entities;

namespace MaxPlus.IPTV.Core.Interfaces;

public interface IPaymentMethodRepository
{
    Task<IEnumerable<PaymentMethod>> GetAllAsync();
    Task<PaymentMethod?>             GetByIdAsync(Guid id);
    Task<Guid>                       CreateAsync(PaymentMethod paymentMethod);
    Task                             UpdateAsync(PaymentMethod paymentMethod);
    Task                             DeleteAsync(Guid id);
}

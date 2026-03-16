using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface IPaymentMethodService
{
    Task<IEnumerable<PaymentMethodResponseDto>> GetAllAsync();
    Task<PaymentMethodResponseDto?>             GetByIdAsync(Guid id);
    Task<Guid>                                  CreateAsync(PaymentMethodCreateDto dto);
    Task                                        UpdateAsync(Guid id, PaymentMethodUpdateDto dto);
    Task                                        DeleteAsync(Guid id);
}

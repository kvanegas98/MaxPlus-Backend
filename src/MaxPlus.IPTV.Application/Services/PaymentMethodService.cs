using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Application.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IPaymentMethodRepository _repository;

    public PaymentMethodService(IPaymentMethodRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PaymentMethodResponseDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(ToDto);
    }

    public async Task<PaymentMethodResponseDto?> GetByIdAsync(Guid id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null ? null : ToDto(item);
    }

    public async Task<Guid> CreateAsync(PaymentMethodCreateDto dto)
    {
        var entity = new PaymentMethod
        {
            Nombre       = dto.Nombre,
            Banco        = dto.Banco,
            TipoCuenta   = dto.TipoCuenta,
            NumeroCuenta = dto.NumeroCuenta,
            Titular      = dto.Titular
        };
        return await _repository.CreateAsync(entity);
    }

    public async Task UpdateAsync(Guid id, PaymentMethodUpdateDto dto)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Método de pago con ID {id} no encontrado.");

        existing.Nombre       = dto.Nombre;
        existing.Banco        = dto.Banco;
        existing.TipoCuenta   = dto.TipoCuenta;
        existing.NumeroCuenta = dto.NumeroCuenta;
        existing.Titular      = dto.Titular;
        existing.IsActive     = dto.IsActive;

        await _repository.UpdateAsync(existing);
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Método de pago con ID {id} no encontrado.");
        await _repository.DeleteAsync(existing.Id);
    }

    private static PaymentMethodResponseDto ToDto(PaymentMethod pm) => new()
    {
        Id           = pm.Id,
        Nombre       = pm.Nombre,
        Banco        = pm.Banco,
        TipoCuenta   = pm.TipoCuenta,
        NumeroCuenta = pm.NumeroCuenta,
        Titular      = pm.Titular,
        IsActive     = pm.IsActive,
        CreatedAt    = pm.CreatedAt
    };
}

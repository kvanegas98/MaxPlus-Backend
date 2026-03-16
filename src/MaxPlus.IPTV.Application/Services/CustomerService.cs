using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IEnumerable<CustomerResponseDto>> GetAllAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return customers.Select(MapToDto);
    }

    public async Task<CustomerResponseDto?> GetByIdAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return customer is null ? null : MapToDto(customer);
    }

    public async Task<IEnumerable<CustomerResponseDto>> SearchAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return await GetAllAsync();

        var customers = await _customerRepository.SearchAsync(term.Trim());
        return customers.Select(MapToDto);
    }

    public async Task<CustomerResponseDto> CreateAsync(CustomerCreateDto dto)
    {
        var customer = new Customer
        {
            Name    = dto.Name.Trim(),
            Phone   = dto.Phone?.Trim(),
            Address = dto.Address?.Trim(),
            Email   = dto.Email?.Trim()
        };

        customer.Id = await _customerRepository.AddAsync(customer);
        return MapToDto(customer);
    }

    public async Task<CustomerResponseDto> UpdateAsync(Guid id, CustomerUpdateDto dto)
    {
        var existing = await _customerRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Cliente con ID {id} no encontrado.");

        existing.Name    = dto.Name.Trim();
        existing.Phone   = dto.Phone?.Trim();
        existing.Address = dto.Address?.Trim();
        existing.Email   = dto.Email?.Trim();

        await _customerRepository.UpdateAsync(existing);
        return MapToDto(existing);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _customerRepository.DeleteAsync(id);
    }

    private static CustomerResponseDto MapToDto(Customer c) => new()
    {
        Id             = c.Id,
        Name           = c.Name,
        Phone          = c.Phone,
        Address        = c.Address,
        Email          = c.Email,
        IsActive       = c.IsActive,
        CreatedAt      = c.CreatedAt
    };
}

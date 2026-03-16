using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(Guid id);
    Task<UserResponseDto>  CreateAsync(UserCreateDto dto);
    Task<UserResponseDto>  UpdateAsync(Guid id, UserUpdateDto dto);
    Task ChangePasswordAsync(Guid id, ChangePasswordDto dto);
    Task DeactivateAsync(Guid id);
}

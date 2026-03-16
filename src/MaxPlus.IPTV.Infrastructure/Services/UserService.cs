using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;

namespace MaxPlus.IPTV.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user is null ? null : MapToDto(user);
    }

    public async Task<UserResponseDto> CreateAsync(UserCreateDto dto)
    {
        var user = new User
        {
            FullName     = dto.FullName.Trim(),
            Email        = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 11),
            RoleId       = dto.RoleId,
            IsActive     = true
        };

        user.Id = await _userRepository.CreateAsync(user);

        return new UserResponseDto
        {
            Id       = user.Id,
            FullName = user.FullName,
            Email    = user.Email,
            RoleId   = user.RoleId,
            IsActive = user.IsActive
        };
    }

    public async Task<UserResponseDto> UpdateAsync(Guid id, UserUpdateDto dto)
    {
        var existing = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Usuario con ID {id} no encontrado.");

        existing.FullName = dto.FullName.Trim();
        existing.Email    = dto.Email.Trim().ToLower();
        existing.RoleId   = dto.RoleId;
        existing.IsActive = dto.IsActive;

        await _userRepository.UpdateAsync(existing);
        return MapToDto(existing);
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Usuario con ID {id} no encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        var newHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 11);
        await _userRepository.ChangePasswordAsync(id, newHash);
    }

    public async Task DeactivateAsync(Guid id)
    {
        await _userRepository.DeactivateAsync(id);
    }

    private static UserResponseDto MapToDto(User u) => new()
    {
        Id          = u.Id,
        FullName    = u.FullName,
        Email       = u.Email,
        RoleId      = u.RoleId,
        RoleName    = u.RoleName,
        IsActive    = u.IsActive,
        CreatedAt   = u.CreatedAt,
        LastLoginAt = u.LastLoginAt
    };
}

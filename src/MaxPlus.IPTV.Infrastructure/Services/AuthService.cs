using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MaxPlus.IPTV.Application.DTOs;
using MaxPlus.IPTV.Application.Interfaces;
using MaxPlus.IPTV.Core.Entities;
using MaxPlus.IPTV.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MaxPlus.IPTV.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IConfiguration  _configuration;

    public AuthService(IUserRepository userRepository, IRoleRepository roleRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _configuration  = configuration;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email.Trim().ToLower());
        if (user is null) return null;

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        await _userRepository.UpdateLastLoginAsync(user.Id);

        return new AuthResponseDto
        {
            Token = GenerateToken(user),
            User  = MapToDto(user)
        };
    }

    public async Task<AuthResponseDto> SetupAsync(SetupDto dto)
    {
        var existing = await _userRepository.GetAllAsync();
        if (existing.Any())
            throw new InvalidOperationException("El setup inicial ya fue completado. Use el login normal.");

        var roles = await _roleRepository.GetAllAsync();
        var adminRole = roles.FirstOrDefault(r => r.Name == "Admin" || r.Name.Contains("Admin", StringComparison.OrdinalIgnoreCase));
        if (adminRole == null)
            throw new InvalidOperationException("No se encontró el rol de Administrador en la base de datos.");

        var user = new User
        {
            FullName     = dto.FullName.Trim(),
            Email        = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 11),
            RoleId       = adminRole.Id,
            RoleName     = adminRole.Name,
            IsActive     = true
        };

        user.Id = await _userRepository.CreateAsync(user);

        return new AuthResponseDto
        {
            Token = GenerateToken(user),
            User  = MapToDto(user)
        };
    }

    private string GenerateToken(User user)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expHours = double.TryParse(_configuration["Jwt:ExpirationHours"], out var h) ? h : 8.0;
        var expires  = DateTime.UtcNow.AddHours(expHours);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name,  user.FullName),
            new Claim(ClaimTypes.Role,               user.RoleName),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             _configuration["Jwt:Issuer"],
            audience:           _configuration["Jwt:Audience"],
            claims:             claims,
            expires:            expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
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

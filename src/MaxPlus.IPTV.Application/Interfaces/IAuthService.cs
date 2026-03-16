using MaxPlus.IPTV.Application.DTOs;

namespace MaxPlus.IPTV.Application.Interfaces;

public interface IAuthService
{
    /// <summary>Login con email + password. Devuelve null si credenciales inválidas.</summary>
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);

    /// <summary>Crea el primer usuario Admin. Solo funciona si no existen usuarios.</summary>
    Task<AuthResponseDto> SetupAsync(SetupDto dto);
}

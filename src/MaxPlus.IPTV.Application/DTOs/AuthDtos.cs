using System.ComponentModel.DataAnnotations;

namespace MaxPlus.IPTV.Application.DTOs;

// ── Login ───────────────────────────────────────────────────
public class LoginDto
{
    [Required(ErrorMessage = "El correo es requerido.")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    public string Email    { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string          Token { get; set; } = string.Empty;
    public UserResponseDto User  { get; set; } = null!;
}

// ── Setup (primer admin) ────────────────────────────────────
public class SetupDto
{
    [Required(ErrorMessage = "El nombre completo es requerido.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es requerido.")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    [MaxLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres.")]
    public string Email    { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;
}

// ── CRUD Usuarios ───────────────────────────────────────────
public class UserResponseDto
{
    public Guid      Id          { get; set; }
    public string    FullName    { get; set; } = string.Empty;
    public string    Email       { get; set; } = string.Empty;
    public Guid      RoleId      { get; set; }
    public string    RoleName    { get; set; } = string.Empty;
    public bool      IsActive    { get; set; }
    public DateTime  CreatedAt   { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UserCreateDto
{
    [Required(ErrorMessage = "El nombre completo es requerido.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es requerido.")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    [MaxLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres.")]
    public string Email    { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es requerido.")]
    public Guid   RoleId   { get; set; }
}

public class UserUpdateDto
{
    [Required(ErrorMessage = "El nombre completo es requerido.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es requerido.")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    [MaxLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres.")]
    public string Email    { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es requerido.")]
    public Guid   RoleId   { get; set; }

    public bool   IsActive { get; set; } = true;
}

public class ChangePasswordDto
{
    [Required(ErrorMessage = "La contraseña actual es requerida.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es requerida.")]
    [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.")]
    public string NewPassword     { get; set; } = string.Empty;
}

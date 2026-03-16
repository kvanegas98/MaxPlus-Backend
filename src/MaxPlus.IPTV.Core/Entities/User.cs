namespace MaxPlus.IPTV.Core.Entities;

public class User
{
    public Guid      Id           { get; set; }
    public string    FullName     { get; set; } = string.Empty;
    public string    Email        { get; set; } = string.Empty;
    public string    PasswordHash { get; set; } = string.Empty;  // BCrypt hash
    public Guid      RoleId       { get; set; }
    public string    RoleName     { get; set; } = string.Empty;
    public bool      IsActive     { get; set; } = true;
    public DateTime  CreatedAt    { get; set; }
    public DateTime? LastLoginAt  { get; set; }
}

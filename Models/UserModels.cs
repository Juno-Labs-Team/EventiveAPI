using Postgrest.Attributes;
using Postgrest.Models;

namespace EventiveAPI.CSharp.Models;

[Table("profiles")]
public class UserProfile : BaseModel
{
    [PrimaryKey("id")]
    public string Id { get; set; } = string.Empty;
    
    [Column("username")]
    public string? Username { get; set; }
    
    [Column("display_name")]
    public string? DisplayName { get; set; }
    
    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }
    
    [Column("bio")]
    public string? Bio { get; set; }
    
    [Column("role")]
    public string Role { get; set; } = "user";
    
    [Column("settings")]
    public Dictionary<string, object>? Settings { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

public class UpdateProfileRequest
{
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
}

public class UpdateSettingsRequest
{
    public Dictionary<string, object> Settings { get; set; } = new();
}

public class AuthUser
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
}

namespace EventiveAPI.CSharp.Configuration;

public class SupabaseConfig
{
    public string Url { get; set; } = string.Empty;
    public string ServiceRoleKey { get; set; } = string.Empty;
    public string AnonKey { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Url))
            throw new ArgumentException("Supabase URL is required");
        
        if (string.IsNullOrWhiteSpace(ServiceRoleKey))
            throw new ArgumentException("Supabase Service Role Key is required");
    }
}

public class CorsConfig
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}

public class StorageConfig
{
    public string AvatarBucket { get; set; } = "avatars";
    public long MaxFileSize { get; set; } = 5242880; // 5MB
}

public class RateLimitConfig
{
    public bool EnableRateLimiting { get; set; } = true;
    public int PermitLimit { get; set; } = 100;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(15);
    public int QueueLimit { get; set; } = 0;
}

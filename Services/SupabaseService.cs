using Supabase;
using EventiveAPI.CSharp.Configuration;

namespace EventiveAPI.CSharp.Services;

public class SupabaseService
{
    private readonly Client _client;
    private readonly SupabaseConfig _config;
    private readonly ILogger<SupabaseService> _logger;

    public SupabaseService(IConfiguration configuration, ILogger<SupabaseService> logger)
    {
        _logger = logger;
 
        _config = new SupabaseConfig
        {
            Url = Environment.GetEnvironmentVariable("SUPABASE_URL") 
                  ?? configuration["Supabase:Url"] ?? "",
                  
            ServiceRoleKey = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY") 
                             ?? configuration["Supabase:ServiceRoleKey"] ?? "",
                             
            AnonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") 
                      ?? configuration["Supabase:AnonKey"] ?? ""
        };
        
        // Now validte the real values
        _config.Validate();

        var options = new SupabaseOptions
        {
            AutoConnectRealtime = false,
            AutoRefreshToken = false
        };

        _client = new Client(_config.Url, _config.ServiceRoleKey, options);
    }

    public Client GetClient() => _client;

    public async Task<Supabase.Gotrue.User?> GetUserFromToken(string token)
    {
        try
        {
            // Use the Supabase GoTrue API to validate the JWT and get the user
            // This calls the /user endpoint which validates the token server-side
            var user = await _client.Auth.GetUser(token);
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to validate token");
            return null;
        }
    }
}

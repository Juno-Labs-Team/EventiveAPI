using Supabase;
using EventiveAPI.CSharp.Configuration;

namespace EventiveAPI.CSharp.Services;

public class SupabaseService
{
    private readonly Client _client;
    private readonly SupabaseConfig _config;

public SupabaseService(IConfiguration configuration)
    {
 
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
            // Create a temporary client with the user's JWT token to validate it
            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false,
                AutoRefreshToken = false
            };
            
            var userClient = new Client(_config.Url, _config.AnonKey, options);
            
            // Set the session with the provided JWT token
            await userClient.Auth.SetSession(token, token);
            
            // Get the user from the session
            var user = userClient.Auth.CurrentUser;
            
            return user;
        }
        catch
        {
            return null;
        }
    }
}

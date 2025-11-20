using Supabase;
using EventiveAPI.CSharp.Configuration;

namespace EventiveAPI.CSharp.Services;

public class SupabaseService
{
    private readonly Client _client;
    private readonly SupabaseConfig _config;

    public SupabaseService(IConfiguration configuration)
    {
        _config = configuration.GetSection("Supabase").Get<SupabaseConfig>() 
            ?? throw new ArgumentNullException(nameof(configuration));
        
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
            var user = await _client.Auth.GetUser(token);
            return user;
        }
        catch
        {
            return null;
        }
    }
}

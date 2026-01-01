using Supabase;
using EventiveAPI.CSharp.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EventiveAPI.CSharp.Services;

public class SupabaseService
{
    private readonly Client _client;
    private readonly SupabaseConfig _config;
    private readonly ILogger<SupabaseService> _logger;
    private readonly HttpClient _httpClient;

    public SupabaseService(IConfiguration configuration, ILogger<SupabaseService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
 
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
            // Make a direct HTTP call to Supabase Auth API to validate the JWT
            var url = $"{_config.Url}/auth/v1/user";
            _logger.LogDebug("Validating token against {Url}", url);
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("apikey", _config.AnonKey);
            
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogDebug("Supabase auth response: {StatusCode} - {Content}", response.StatusCode, content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token validation failed with status {StatusCode}: {Content}", response.StatusCode, content);
                return null;
            }
            
            var userResponse = JsonSerializer.Deserialize<SupabaseUserResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (userResponse == null || string.IsNullOrEmpty(userResponse.Id))
            {
                _logger.LogWarning("Failed to deserialize user response or missing ID");
                return null;
            }
            
            _logger.LogDebug("Successfully validated token for user {UserId}", userResponse.Id);
            
            // Convert to Supabase.Gotrue.User
            return new Supabase.Gotrue.User
            {
                Id = userResponse.Id,
                Email = userResponse.Email
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate token: {Message}", ex.Message);
            return null;
        }
    }
}

// Helper class to deserialize Supabase user response
public class SupabaseUserResponse
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
}

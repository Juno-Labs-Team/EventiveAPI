using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EventiveAPI.CSharp.Models;
using EventiveAPI.CSharp.Services;

namespace EventiveAPI.CSharp.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly SupabaseService _supabaseService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(SupabaseService supabaseService, ILogger<SettingsController> logger)
    {
        _supabaseService = supabaseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, error = new { message = "Not authenticated" } });
            }

            var client = _supabaseService.GetClient();
            var profile = await client
                .From<UserProfile>()
                .Select("settings")
                .Where(p => p.Id == userId)
                .Single();

            if (profile == null)
            {
                return NotFound(new { success = false, error = new { message = "User not found" } });
            }

            var settings = profile.Settings ?? new Dictionary<string, object>();
            return Ok(new { success = true, data = settings });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching settings");
            return StatusCode(500, new { success = false, error = new { message = "Failed to fetch settings" } });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, error = new { message = "Not authenticated" } });
            }

            var client = _supabaseService.GetClient();
            
            // Fetch current profile
            var profile = await client
                .From<UserProfile>()
                .Where(p => p.Id == userId)
                .Single();

            if (profile == null)
            {
                return NotFound(new { success = false, error = new { message = "User not found" } });
            }

            profile.Settings = request.Settings;
            profile.UpdatedAt = DateTime.UtcNow;

            var response = await client
                .From<UserProfile>()
                .Update(profile); 

            var updated = response.Models.FirstOrDefault();

            if (updated == null)
            {
                return NotFound(new { success = false, error = new { message = "User not found" } });
            }

            return Ok(new { success = true, data = updated.Settings ?? new Dictionary<string, object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings");
            return StatusCode(500, new { success = false, error = new { message = "Failed to update settings" } });
        }
    }
}

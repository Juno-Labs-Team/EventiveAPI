using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EventiveAPI.CSharp.Models;
using EventiveAPI.CSharp.Services;

namespace EventiveAPI.CSharp.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly SupabaseService _supabaseService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(SupabaseService supabaseService, ILogger<UsersController> logger)
    {
        _supabaseService = supabaseService;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
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
                .Where(p => p.Id == userId)
                .Single();

            if (profile == null)
            {
                return NotFound(new { success = false, error = new { message = "User profile not found" } });
            }

            return Ok(new { success = true, data = profile });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current user");
            return StatusCode(500, new { success = false, error = new { message = "Failed to fetch user profile" } });
        }
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateProfileRequest request)
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
                return NotFound(new { success = false, error = new { message = "User profile not found" } });
            }

            // Update fields
            if (request.Username != null)
                profile.Username = request.Username;
            
            if (request.DisplayName != null)
                profile.DisplayName = request.DisplayName;
            
            if (request.Bio != null)
                profile.Bio = request.Bio;

            profile.UpdatedAt = DateTime.UtcNow;

            var response = await client
                .From<UserProfile>()
                .Update(profile);

            var updated = response.Models.FirstOrDefault();

            if (updated == null)
            {
                return NotFound(new { success = false, error = new { message = "User profile not found" } });
            }

            return Ok(new { success = true, data = updated });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { success = false, error = new { message = "Failed to update user profile" } });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        try
        {
            var client = _supabaseService.GetClient();
            var profile = await client
                .From<UserProfile>()
                .Select("id, username, display_name, avatar_url, bio, created_at")
                .Where(p => p.Id == id)
                .Single();

            if (profile == null)
            {
                return NotFound(new { success = false, error = new { message = "User not found" } });
            }

            return Ok(new { success = true, data = profile });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by ID");
            return StatusCode(500, new { success = false, error = new { message = "Failed to fetch user" } });
        }
    }
}

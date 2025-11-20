using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EventiveAPI.CSharp.Services;
using EventiveAPI.CSharp.Configuration;

namespace EventiveAPI.CSharp.Controllers;

[ApiController]
[Route("api/uploads")]
public class UploadsController : ControllerBase
{
    private readonly SupabaseService _supabaseService;
    private readonly ILogger<UploadsController> _logger;
    private readonly IConfiguration _configuration;
    private readonly long _maxFileSize;
    private readonly string _avatarBucket;

    public UploadsController(
        SupabaseService supabaseService, 
        ILogger<UploadsController> logger,
        IConfiguration configuration)
    {
        _supabaseService = supabaseService;
        _logger = logger;
        _configuration = configuration;
        
        var storageConfig = configuration.GetSection("Storage").Get<StorageConfig>() ?? new StorageConfig();
        _maxFileSize = storageConfig.MaxFileSize;
        _avatarBucket = storageConfig.AvatarBucket;
    }

    [HttpPost("avatar")]
    [RequestSizeLimit(10485760)] // 10MB max
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, error = new { message = "Not authenticated" } });
            }

            // Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, error = new { message = "No file provided" } });
            }

            if (file.Length > _maxFileSize)
            {
                return BadRequest(new { success = false, error = new { message = $"File size exceeds maximum allowed size of {_maxFileSize / 1024 / 1024}MB" } });
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new { success = false, error = new { message = "Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed" } });
            }

            var client = _supabaseService.GetClient();
            
            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{userId}/{Guid.NewGuid()}{fileExtension}";

            // Delete old avatar if exists
            var profile = await client
                .From<Models.UserProfile>()
                .Where(p => p.Id == userId)
                .Single();

            if (profile?.AvatarUrl != null && !string.IsNullOrEmpty(profile.AvatarUrl))
            {
                try
                {
                    // Extract file path from URL
                    var oldPath = ExtractFilePathFromUrl(profile.AvatarUrl, _avatarBucket);
                    if (!string.IsNullOrEmpty(oldPath))
                    {
                        await client.Storage.From(_avatarBucket).Remove(oldPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old avatar");
                }
            }

            // Upload new avatar
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            await client.Storage
                .From(_avatarBucket)
                .Upload(fileBytes, fileName, new Supabase.Storage.FileOptions
                {
                    ContentType = file.ContentType,
                    Upsert = true
                });

            // Get public URL
            var publicUrl = client.Storage.From(_avatarBucket).GetPublicUrl(fileName);

            // Update user profile
            if (profile != null)
            {
                profile.AvatarUrl = publicUrl;
                profile.UpdatedAt = DateTime.UtcNow;

                await client
                    .From<Models.UserProfile>()
                    .Update(profile);
            }

            return Ok(new { 
                success = true, 
                data = new { url = publicUrl } 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar");
            return StatusCode(500, new { success = false, error = new { message = "Failed to upload avatar" } });
        }
    }

    [HttpDelete("avatar")]
    public async Task<IActionResult> DeleteAvatar()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, error = new { message = "Not authenticated" } });
            }

            var client = _supabaseService.GetClient();
            
            // Get current profile
            var profile = await client
                .From<Models.UserProfile>()
                .Where(p => p.Id == userId)
                .Single();

            if (profile?.AvatarUrl == null || string.IsNullOrEmpty(profile.AvatarUrl))
            {
                return BadRequest(new { success = false, error = new { message = "No avatar to delete" } });
            }

            // Delete from storage
            var filePath = ExtractFilePathFromUrl(profile.AvatarUrl, _avatarBucket);
            if (!string.IsNullOrEmpty(filePath))
            {
                await client.Storage.From(_avatarBucket).Remove(filePath);
            }

            // Update profile
            profile.AvatarUrl = null;
            profile.UpdatedAt = DateTime.UtcNow;

            await client
                .From<Models.UserProfile>()
                .Update(profile);

            return Ok(new { success = true, data = new { message = "Avatar deleted successfully" } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting avatar");
            return StatusCode(500, new { success = false, error = new { message = "Failed to delete avatar" } });
        }
    }

    private string ExtractFilePathFromUrl(string url, string bucket)
    {
        try
        {
            // Extract path after bucket name in URL
            var bucketSegment = $"/storage/v1/object/public/{bucket}/";
            var index = url.IndexOf(bucketSegment);
            if (index >= 0)
            {
                return url.Substring(index + bucketSegment.Length);
            }
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}

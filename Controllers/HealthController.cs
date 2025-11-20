using Microsoft.AspNetCore.Mvc;

namespace EventiveAPI.CSharp.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    [HttpGet("docs")]
    public IActionResult Docs()
    {
        return Ok(new { 
            message = "API Documentation",
            swagger = "/swagger",
            endpoints = new
            {
                health = "GET /health",
                users = new
                {
                    getCurrentUser = "GET /api/users/me",
                    updateCurrentUser = "PUT /api/users/me",
                    getUserById = "GET /api/users/{id}"
                },
                settings = new
                {
                    getSettings = "GET /api/settings",
                    updateSettings = "PUT /api/settings"
                },
                uploads = new
                {
                    uploadAvatar = "POST /api/uploads/avatar",
                    deleteAvatar = "DELETE /api/uploads/avatar"
                },
                auth = new
                {
                    callback = "POST /api/auth/callback",
                    refresh = "POST /api/auth/refresh",
                    logout = "POST /api/auth/logout"
                }
            }
        });
    }
}

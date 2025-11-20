using Microsoft.AspNetCore.Mvc;

namespace EventiveAPI.CSharp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpPost("callback")]
    public IActionResult Callback()
    {
        // OAuth callback is handled by Supabase on the frontend
        return Ok(new { 
            success = true, 
            data = new { message = "OAuth callback - handled by Supabase client" } 
        });
    }

    [HttpPost("refresh")]
    public IActionResult RefreshToken()
    {
        // Token refresh is handled by Supabase on the frontend
        return Ok(new { 
            success = true, 
            data = new { message = "Token refresh - handled by Supabase client" } 
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Logout is handled by Supabase on the frontend
        return Ok(new { 
            success = true, 
            data = new { message = "Logout - handled by Supabase client" } 
        });
    }
}

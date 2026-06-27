using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using System.Security.Claims;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>UC-05: Đăng ký tài khoản</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Đăng ký thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<AuthResponse>.Fail(ex.Message));
        }
    }

    /// <summary>UC-06: Đăng nhập (3 role: Customer/Staff/Admin)</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Đăng nhập thành công."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail(ex.Message));
        }
    }

    /// <summary>UC-09: Đăng xuất (stateless JWT — client tự xóa token)</summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(ApiResponse<string>.Ok(string.Empty, "Đăng xuất thành công."));
    }

    /// <summary>Lấy thông tin user đang đăng nhập</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email");
        var fullName = User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("name");
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(ApiResponse<object>.Ok(new { userId, email, fullName, role }));
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using System.Security.Claims;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/users/me")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _profileService;

    public UserProfileController(IUserProfileService profileService)
    {
        _profileService = profileService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());

    /// <summary>UC-10: Xem profile của user đang đăng nhập</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var result = await _profileService.GetProfileAsync(GetUserId());
            return Ok(ApiResponse<UserProfileResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserProfileResponse>.Fail(ex.Message));
        }
    }

    /// <summary>UC-10: Cập nhật profile (avatar, info)</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var result = await _profileService.UpdateProfileAsync(GetUserId(), request);
            return Ok(ApiResponse<UserProfileResponse>.Ok(result, "Cập nhật profile thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserProfileResponse>.Fail(ex.Message));
        }
    }
}

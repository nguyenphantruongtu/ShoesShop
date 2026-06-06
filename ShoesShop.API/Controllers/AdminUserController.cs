using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.API.Models;
using ShoesShop.Business.Constants;
using ShoesShop.Business.DTOs.Admin;
using ShoesShop.Business.DTOs.User;
using ShoesShop.Business.Interfaces;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = Roles.Admin)]
public class AdminUserController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUserController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    /// <summary>UC-38: Danh sách user (search + phân trang)</summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _adminUserService.GetUsersAsync(search, page, pageSize);
        return Ok(ApiResponse<AdminUserListResponse>.Ok(result));
    }

    /// <summary>UC-38: Khóa tài khoản user</summary>
    [HttpPatch("{userId:int}/lock")]
    public async Task<IActionResult> Lock(int userId)
    {
        try
        {
            var result = await _adminUserService.LockUserAsync(userId);
            return Ok(ApiResponse<UserProfileResponse>.Ok(result, $"Đã khóa tài khoản UserId={userId}."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserProfileResponse>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserProfileResponse>.Fail(ex.Message));
        }
    }

    /// <summary>UC-38: Mở khóa tài khoản user</summary>
    [HttpPatch("{userId:int}/unlock")]
    public async Task<IActionResult> Unlock(int userId)
    {
        try
        {
            var result = await _adminUserService.UnlockUserAsync(userId);
            return Ok(ApiResponse<UserProfileResponse>.Ok(result, $"Đã mở khóa tài khoản UserId={userId}."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserProfileResponse>.Fail(ex.Message));
        }
    }

    /// <summary>UC-38: Đổi role của user</summary>
    [HttpPatch("{userId:int}/role")]
    public async Task<IActionResult> UpdateRole(int userId, [FromBody] AdminUpdateRoleRequest request)
    {
        try
        {
            var result = await _adminUserService.UpdateRoleAsync(userId, request);
            return Ok(ApiResponse<UserProfileResponse>.Ok(result, "Đổi role thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserProfileResponse>.Fail(ex.Message));
        }
    }
}

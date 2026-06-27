namespace ShoesShop.Business.Interfaces;

public interface IAdminUserService
{
    Task<AdminUserListResponse> GetUsersAsync(string? search, int page, int pageSize);
    Task<UserProfileResponse> LockUserAsync(int userId);
    Task<UserProfileResponse> UnlockUserAsync(int userId);
    Task<UserProfileResponse> UpdateRoleAsync(int userId, AdminUpdateRoleRequest request);
}

using ShoesShop.Shared.DTOs.Auth;

namespace ShoesShop.Business.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileResponse> GetProfileAsync(int userId);
    Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);

    /// <summary>UC-11: Đổi mật khẩu khi đã đăng nhập</summary>
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
}

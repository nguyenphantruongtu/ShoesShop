namespace ShoesShop.Business.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileResponse> GetProfileAsync(int userId);
    Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
}

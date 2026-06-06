using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Business.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepository;

    public UserProfileService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileResponse> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        return MapToResponse(user);
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetUserByIdAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.FullName = request.FullName;
        user.Phone = request.Phone;
        user.DateOfBirth = request.DateOfBirth;
        user.Gender = request.Gender;
        user.AvatarUrl = request.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateUserAsync(user);

        return MapToResponse(user);
    }

    private static UserProfileResponse MapToResponse(Data.Entities.User user) => new()
    {
        UserId = user.UserId,
        Email = user.Email,
        FullName = user.FullName,
        Phone = user.Phone,
        DateOfBirth = user.DateOfBirth,
        Gender = user.Gender,
        AvatarUrl = user.AvatarUrl,
        Role = user.Role.RoleName,
        IsActive = user.IsActive,
        EmailConfirmed = user.EmailConfirmed,
        CreatedAt = user.CreatedAt
    };
}

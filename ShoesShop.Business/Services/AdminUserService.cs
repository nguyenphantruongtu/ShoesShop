using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Business.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public AdminUserService(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<AdminUserListResponse> GetUsersAsync(string? search, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize;

        var (users, total) = await _userRepository.GetUsersPaginatedAsync(search, page, pageSize);

        return new AdminUserListResponse
        {
            Users = users.Select(u => new AdminUserListItem
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Role = u.Role.RoleName,
                RoleId = u.RoleId,
                IsActive = u.IsActive,
                EmailConfirmed = u.EmailConfirmed,
                CreatedAt = u.CreatedAt
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UserProfileResponse> LockUserAsync(int userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        if (user.Role.RoleName == "Admin")
            throw new InvalidOperationException("Không thể khóa tài khoản Admin.");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user);

        return MapToProfile(user);
    }

    public async Task<UserProfileResponse> UnlockUserAsync(int userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user);

        return MapToProfile(user);
    }

    public async Task<UserProfileResponse> UpdateRoleAsync(int userId, AdminUpdateRoleRequest request)
    {
        var user = await _userRepository.GetUserByIdAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        var role = await _roleRepository.GetRoleByNameAsync(request.RoleName)
            ?? throw new KeyNotFoundException($"Role '{request.RoleName}' không tồn tại.");

        user.RoleId = role.RoleId;
        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user);

        return MapToProfile(user);
    }

    private static UserProfileResponse MapToProfile(Data.Entities.User user) => new()
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

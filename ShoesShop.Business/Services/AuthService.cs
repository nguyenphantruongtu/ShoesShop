using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IRoleRepository roleRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.IsEmailExistsAsync(request.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var customerRole = await _roleRepository.GetRoleByNameAsync(Roles.Customer)
            ?? throw new InvalidOperationException("Role 'Customer' chưa được seed vào database.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Phone = request.Phone,
            RoleId = customerRole.RoleId,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddUserAsync(user);

        // Gán Role navigation property để GenerateToken có thể đọc RoleName
        user.Role = customerRole;

        return new AuthResponse
        {
            Token = _jwtService.GenerateToken(user),
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            Role = customerRole.RoleName,
            ExpiresAt = _jwtService.GetExpiry()
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetUserByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản đã bị khóa. Vui lòng liên hệ hỗ trợ.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        return new AuthResponse
        {
            Token = _jwtService.GenerateToken(user),
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.RoleName,
            ExpiresAt = _jwtService.GetExpiry()
        };
    }
}

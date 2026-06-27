using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Shared.DTOs.Auth;

/// <summary>UC-11: Đổi mật khẩu khi đã đăng nhập</summary>
public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}

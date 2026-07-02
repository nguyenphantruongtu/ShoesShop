using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Shared.DTOs.User;

public class UpdateProfileRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.")]
    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }
}

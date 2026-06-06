using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Business.DTOs.User;

public class UpdateProfileRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }
}

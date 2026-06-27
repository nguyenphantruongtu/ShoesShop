using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Web.Models.Profile;

public class UpdateProfileViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [MaxLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = null!;

    [MaxLength(20, ErrorMessage = "Số điện thoại không quá 20 ký tự")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [Display(Name = "Ngày sinh")]
    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(10)]
    [Display(Name = "Giới tính")]
    public string? Gender { get; set; }

    [MaxLength(500)]
    [Display(Name = "Ảnh đại diện (URL)")]
    public string? AvatarUrl { get; set; }
}

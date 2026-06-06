using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Web.Models.Profile;

/// <summary>Dùng cho cả tạo mới và cập nhật địa chỉ. AddressId = 0 → tạo mới.</summary>
public class AddressFormViewModel
{
    public int AddressId { get; set; } // 0 = create, >0 = update

    [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
    [MaxLength(100)]
    [Display(Name = "Tên người nhận")]
    public string RecipientName { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [MaxLength(20)]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập tỉnh/thành phố")]
    [MaxLength(100)]
    [Display(Name = "Tỉnh / Thành phố")]
    public string Province { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập quận/huyện")]
    [MaxLength(100)]
    [Display(Name = "Quận / Huyện")]
    public string District { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập phường/xã")]
    [MaxLength(100)]
    [Display(Name = "Phường / Xã")]
    public string Ward { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể")]
    [MaxLength(255)]
    [Display(Name = "Địa chỉ chi tiết")]
    public string StreetAddress { get; set; } = null!;

    [Display(Name = "Đặt làm địa chỉ mặc định")]
    public bool IsDefault { get; set; }
}

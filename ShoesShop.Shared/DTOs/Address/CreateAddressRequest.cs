using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Shared.DTOs.Address;

public class CreateAddressRequest
{
    [Required]
    [MaxLength(100)]
    public string RecipientName { get; set; } = null!;

    [Required]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.")]
    public string Phone { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Province { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string District { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Ward { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string StreetAddress { get; set; } = null!;

    public bool IsDefault { get; set; } = false;
}

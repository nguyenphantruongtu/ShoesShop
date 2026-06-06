using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Business.DTOs.Address;

public class UpdateAddressRequest
{
    [Required]
    [MaxLength(100)]
    public string RecipientName { get; set; } = null!;

    [Required]
    [MaxLength(20)]
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

    public bool IsDefault { get; set; }
}

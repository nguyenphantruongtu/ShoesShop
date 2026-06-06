using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Business.DTOs.Admin;

public class AdminUpdateRoleRequest
{
    [Required]
    public string RoleName { get; set; } = null!;
}

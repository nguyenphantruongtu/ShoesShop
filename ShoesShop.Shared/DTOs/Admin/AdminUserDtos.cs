using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Shared.DTOs.Admin;

public class AdminUserListItem
{
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string Role { get; set; } = null!;
    public int RoleId { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminUserListResponse
{
    public List<AdminUserListItem> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class AdminUpdateRoleRequest
{
    [Required]
    public string RoleName { get; set; } = null!;
}

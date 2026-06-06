namespace ShoesShop.Shared.DTOs;

public class RevenueGroupDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = null!;
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}

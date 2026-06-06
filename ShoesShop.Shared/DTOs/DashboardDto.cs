namespace ShoesShop.Shared.DTOs;

public class DashboardDto
{
    public decimal RevenueToday { get; set; }
    public decimal RevenueWeek { get; set; }
    public decimal RevenueMonth { get; set; }

    public int OrdersToday { get; set; }
    public int OrdersWeek { get; set; }
    public int OrdersMonth { get; set; }

    // For chart: labels and data for last 7 days (client-friendly)
    public List<string> Last7DaysLabels { get; set; } = new();
    public List<decimal> Last7DaysRevenue { get; set; } = new();
}

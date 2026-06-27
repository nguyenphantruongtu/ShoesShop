namespace ShoesShop.Shared.DTOs.Dashboard;

public class DashboardSummaryResponse
{
    public RevenueStats Revenue { get; set; } = new();
    public OrderStats Orders { get; set; } = new();
    public List<DailyRevenuePoint> RevenueChart { get; set; } = new();
    public List<TopProductItem> TopProducts { get; set; } = new();
}

public class RevenueStats
{
    public decimal Today { get; set; }
    public decimal ThisWeek { get; set; }
    public decimal ThisMonth { get; set; }
}

public class OrderStats
{
    public int TotalOrders { get; set; }
    public int Pending { get; set; }
    public int Confirmed { get; set; }
    public int Preparing { get; set; }
    public int Shipping { get; set; }
    public int Delivered { get; set; }
    public int Cancelled { get; set; }
}

public class DailyRevenuePoint
{
    public DateOnly Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopProductItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}

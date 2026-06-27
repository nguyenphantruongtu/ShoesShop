using ShoesShop.Shared.DTOs.Dashboard;

namespace ShoesShop.Business.Interfaces;

public interface IDashboardService
{
    /// <summary>UC-39: Tổng quan dashboard — doanh thu, số đơn, biểu đồ, top sản phẩm</summary>
    Task<DashboardSummaryResponse> GetSummaryAsync();
}

using ShoesShop.Shared.DTOs;

namespace ShoesShop.Business.Interfaces;

public interface IReportService
{
    Task<IEnumerable<TopProductDto>> GetTopSellingProductsAsync(int top = 10, DateTime? from = null, DateTime? to = null);

    // New methods for grouping revenue
    Task<IEnumerable<RevenueGroupDto>> GetRevenueByBrandAsync(DateTime? from = null, DateTime? to = null, int top = 20);
    Task<IEnumerable<RevenueGroupDto>> GetRevenueByCategoryAsync(DateTime? from = null, DateTime? to = null, int top = 20);
}

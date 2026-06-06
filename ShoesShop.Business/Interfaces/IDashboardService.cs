using ShoesShop.Shared.DTOs;

namespace ShoesShop.Business.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetOverviewAsync();
}

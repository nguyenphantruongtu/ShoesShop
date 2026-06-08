using ShoesShop.Shared.DTOs;
using System.Threading.Tasks;

namespace ShoesShop.Business.Interfaces;

public interface IOrderService
{
    Task<int> CreateOrderAsync(OrderInputDto input);
    Task<bool> CancelOrderAsync(int orderId, int userId);
    Task<List<OrderDto>> GetOrderHistoryByUserIdAsync(int userId);
}
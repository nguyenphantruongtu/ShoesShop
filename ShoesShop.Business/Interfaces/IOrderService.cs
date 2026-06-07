using ShoesShop.Shared.DTOs;
using System.Threading.Tasks;

namespace ShoesShop.Business.Interfaces;

public interface IOrderService
{
    Task<int> CreateOrderAsync(OrderInputDto input);
}
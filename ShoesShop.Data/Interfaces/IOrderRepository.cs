using System.Threading.Tasks;
using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Interfaces;

public interface IOrderRepository
{
    Task AddAsync<T>(T entity) where T : class;
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<Order?> GetByIdAsync(int orderId);
    Task UpdateAsync(Order order);
    Task<List<Order>> GetOrdersByUserIdAsync(int userId);
}
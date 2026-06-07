using System.Threading.Tasks;

namespace ShoesShop.Data.Interfaces;

public interface IOrderRepository
{
    // Cần một hàm để add đối tượng bất kỳ hoặc add Order trực tiếp
    Task AddAsync<T>(T entity) where T : class;
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
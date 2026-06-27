using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Repositories.Interfaces;

public interface ISizeRepository
{
    Task<List<Size>> GetAllAsync();
    Task<Size?> GetByIdAsync(int id);
    Task<bool> ValueExistsAsync(string value, int? excludeId = null);
    Task AddAsync(Size size);
    Task UpdateAsync(Size size);
    Task DeleteAsync(Size size);
}

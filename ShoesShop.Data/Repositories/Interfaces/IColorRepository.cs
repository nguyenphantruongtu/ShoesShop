using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Repositories.Interfaces;

public interface IColorRepository
{
    Task<List<Color>> GetAllAsync();
    Task<Color?> GetByIdAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task AddAsync(Color color);
    Task UpdateAsync(Color color);
    Task DeleteAsync(Color color);
}

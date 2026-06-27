using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetByIdWithChildrenAsync(int id);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
    Task<bool> HasChildrenAsync(int id);
    Task<bool> HasProductsAsync(int id);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
}

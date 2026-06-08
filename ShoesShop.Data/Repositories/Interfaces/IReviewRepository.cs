using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Repositories.Interfaces;

public interface IReviewRepository
{
    Task<List<Review>> GetByProductIdAsync(int productId);
    Task<Review?> GetByIdAsync(int reviewId);
    Task<bool> HasReviewedOrderItemAsync(int userId, int orderItemId);
    Task AddAsync(Review review);
    Task UpdateAsync(Review review);
}

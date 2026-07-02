using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Repositories.Interfaces;

public interface IReviewRepository
{
    Task<List<Review>> GetByProductIdAsync(int productId);
    Task<Review?> GetByIdAsync(int reviewId);
    Task<bool> HasReviewedOrderItemAsync(int userId, int orderItemId);
    Task AddAsync(Review review);
    Task UpdateAsync(Review review);
    Task DeleteAsync(Review review);

    /// <summary>UC-24: Danh sách đánh giá cho Admin/Staff kiểm duyệt — lọc theo trạng thái duyệt</summary>
    Task<(List<Review> Reviews, int TotalCount)> GetForAdminAsync(bool? isApproved, int page, int pageSize);
}

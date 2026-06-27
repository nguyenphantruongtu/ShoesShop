using ShoesShop.Shared.DTOs.Review;

namespace ShoesShop.Business.Interfaces;

public interface IReviewService
{
    /// <summary>UC-23: Customer gửi đánh giá (chỉ sau khi Delivered + verified purchase)</summary>
    Task<ReviewResponse> CreateAsync(int userId, CreateReviewRequest request);

    /// <summary>Lấy danh sách review của sản phẩm (public)</summary>
    Task<ProductReviewSummary> GetByProductIdAsync(int productId);
}

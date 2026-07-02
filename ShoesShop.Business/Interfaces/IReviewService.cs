using ShoesShop.Shared.DTOs.Review;

namespace ShoesShop.Business.Interfaces;

public interface IReviewService
{
    /// <summary>UC-23: Customer gửi đánh giá (chỉ sau khi Delivered + verified purchase)</summary>
    Task<ReviewResponse> CreateAsync(int userId, CreateReviewRequest request);

    /// <summary>Lấy danh sách review của sản phẩm (public — chỉ trả review đã duyệt)</summary>
    Task<ProductReviewSummary> GetByProductIdAsync(int productId);

    /// <summary>UC-24: Admin/Staff xem tất cả đánh giá để kiểm duyệt (lọc theo trạng thái duyệt)</summary>
    Task<AdminReviewListResponse> GetForAdminAsync(bool? isApproved, int page, int pageSize);

    /// <summary>UC-24: Duyệt hoặc ẩn một đánh giá (đánh giá tốt → duyệt để hiển thị công khai)</summary>
    Task<ReviewResponse> SetApprovalAsync(int reviewId, bool isApproved);

    /// <summary>UC-24: Xóa đánh giá vi phạm/spam</summary>
    Task DeleteAsync(int reviewId);
}

using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Shared.DTOs.Review;

// ── RESPONSE DTOs ──────────────────────────────────────────────────────────────

public class ReviewResponse
{
    public int ReviewId { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string? UserAvatar { get; set; }
    public int? OrderItemId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? ImageUrls { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductReviewSummary
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<ReviewResponse> Reviews { get; set; } = new();
}

/// <summary>Đánh giá hiển thị trong danh sách kiểm duyệt của Admin/Staff</summary>
public class AdminReviewListItem
{
    public int ReviewId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductImageUrl { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public int? OrderItemId { get; set; }
    /// <summary>True nếu đánh giá gắn với một đơn hàng thật (đã mua + Delivered); false nếu là dữ liệu seed/demo.</summary>
    public bool IsVerifiedPurchase { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? ImageUrls { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminReviewListResponse
{
    public List<AdminReviewListItem> Reviews { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

// ── REQUEST DTOs ───────────────────────────────────────────────────────────────

/// <summary>
/// UC-23: Customer gửi đánh giá sản phẩm sau khi đơn Delivered.
/// ProductId KHÔNG nhận từ client — server tự suy ra từ OrderItem để tránh giả mạo.
/// </summary>
public class CreateReviewRequest
{
    /// <summary>OrderItemId để verify đã mua hàng + suy ra ProductId</summary>
    [Required]
    public int OrderItemId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public string? ImageUrls { get; set; }
}

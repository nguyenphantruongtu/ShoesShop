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

// ── REQUEST DTOs ───────────────────────────────────────────────────────────────

/// <summary>UC-23: Customer gửi đánh giá sản phẩm sau khi đơn Delivered</summary>
public class CreateReviewRequest
{
    [Required]
    public int ProductId { get; set; }

    /// <summary>OrderItemId để verify đã mua hàng</summary>
    [Required]
    public int OrderItemId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public string? ImageUrls { get; set; }
}

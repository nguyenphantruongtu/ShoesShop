using Microsoft.EntityFrameworkCore;
using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;
using ShoesShop.Shared.DTOs.Review;

namespace ShoesShop.Business.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _repo;
    private readonly ShoeStoreDbContext _ctx;

    public ReviewService(IReviewRepository repo, ShoeStoreDbContext ctx)
    {
        _repo = repo;
        _ctx  = ctx;
    }

    // ── UC-23: Customer gửi đánh giá (chỉ sau khi Delivered + verified purchase) ────
    public async Task<ReviewResponse> CreateAsync(int userId, CreateReviewRequest request)
    {
        // Xác minh order item thuộc về user này + lấy ProductId từ Variant (không tin client)
        var orderItem = await _ctx.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Variant)
            .FirstOrDefaultAsync(oi => oi.OrderItemId == request.OrderItemId
                                       && oi.Order.UserId == userId);

        if (orderItem == null)
            throw new KeyNotFoundException("Không tìm thấy sản phẩm trong đơn hàng.");

        if (orderItem.Order.OrderStatus != "Delivered")
            throw new InvalidOperationException("Chỉ có thể đánh giá sau khi đơn hàng đã giao thành công.");

        if (orderItem.Variant == null)
            throw new InvalidOperationException("Không xác định được sản phẩm để đánh giá.");

        if (await _repo.HasReviewedOrderItemAsync(userId, request.OrderItemId))
            throw new InvalidOperationException("Bạn đã đánh giá sản phẩm này rồi.");

        var review = new Review
        {
            ProductId   = orderItem.Variant.ProductId,
            UserId      = userId,
            OrderItemId = request.OrderItemId,
            Rating      = request.Rating,
            Comment     = request.Comment,
            ImageUrls   = request.ImageUrls,
            // Đánh giá mới luôn ở trạng thái "đã gửi" — chờ Admin/Staff kiểm duyệt trước khi hiển thị công khai.
            IsApproved  = false,
            CreatedAt   = DateTime.UtcNow
        };

        await _repo.AddAsync(review);

        var user = await _ctx.Users.FindAsync(userId);
        return MapToResponse(review, user?.FullName, user?.AvatarUrl);
    }

    // ── Public: danh sách review đã duyệt của sản phẩm ──────────────────────
    public async Task<ProductReviewSummary> GetByProductIdAsync(int productId)
    {
        var reviews = await _repo.GetByProductIdAsync(productId);

        return new ProductReviewSummary
        {
            TotalReviews  = reviews.Count,
            AverageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0,
            Reviews       = reviews.Select(r => MapToResponse(r, r.User?.FullName, r.User?.AvatarUrl)).ToList()
        };
    }

    // ── UC-24: Admin/Staff kiểm duyệt ────────────────────────────────────────
    public async Task<AdminReviewListResponse> GetForAdminAsync(bool? isApproved, int page, int pageSize)
    {
        page     = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize;

        var (reviews, total) = await _repo.GetForAdminAsync(isApproved, page, pageSize);

        return new AdminReviewListResponse
        {
            Reviews = reviews.Select(r => new AdminReviewListItem
            {
                ReviewId           = r.ReviewId,
                ProductId          = r.ProductId,
                ProductName        = r.Product?.ProductName ?? "",
                ProductImageUrl    = r.Product?.ProductImages
                                        .OrderByDescending(img => img.IsPrimary)
                                        .Select(img => img.ImageUrl)
                                        .FirstOrDefault(),
                UserId             = r.UserId,
                UserName           = r.User?.FullName ?? "",
                OrderItemId        = r.OrderItemId,
                IsVerifiedPurchase = r.OrderItemId.HasValue,
                Rating             = r.Rating,
                Comment            = r.Comment,
                ImageUrls          = r.ImageUrls,
                IsApproved         = r.IsApproved,
                CreatedAt          = r.CreatedAt
            }).ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    public async Task<ReviewResponse> SetApprovalAsync(int reviewId, bool isApproved)
    {
        var review = await _repo.GetByIdAsync(reviewId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đánh giá #{reviewId}.");

        review.IsApproved = isApproved;
        await _repo.UpdateAsync(review);

        return MapToResponse(review, review.User?.FullName, review.User?.AvatarUrl);
    }

    public async Task DeleteAsync(int reviewId)
    {
        var review = await _repo.GetByIdAsync(reviewId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đánh giá #{reviewId}.");

        await _repo.DeleteAsync(review);
    }

    // ── Mapping ──────────────────────────────────────────────────────────────
    private static ReviewResponse MapToResponse(Review r, string? userName, string? userAvatar) => new()
    {
        ReviewId    = r.ReviewId,
        ProductId   = r.ProductId,
        UserId      = r.UserId,
        UserName    = userName ?? "",
        UserAvatar  = userAvatar,
        OrderItemId = r.OrderItemId,
        Rating      = r.Rating,
        Comment     = r.Comment,
        ImageUrls   = r.ImageUrls,
        IsApproved  = r.IsApproved,
        CreatedAt   = r.CreatedAt
    };
}

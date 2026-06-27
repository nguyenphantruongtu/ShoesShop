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

    public async Task<ReviewResponse> CreateAsync(int userId, CreateReviewRequest request)
    {
        // Verify order item belongs to this user and order is Delivered
        var orderItem = await _ctx.OrderItems
            .Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi => oi.OrderItemId == request.OrderItemId
                                       && oi.Order.UserId == userId);

        if (orderItem == null)
            throw new KeyNotFoundException("Không tìm thấy sản phẩm trong đơn hàng.");

        if (orderItem.Order.OrderStatus != "Delivered")
            throw new InvalidOperationException("Chỉ có thể đánh giá sau khi đơn hàng được giao.");

        // Check product matches
        if (orderItem.Variant == null)
        {
            var variant = await _ctx.ProductVariants
                .FirstOrDefaultAsync(v => v.VariantId == orderItem.VariantId);
            if (variant?.ProductId != request.ProductId)
                throw new ArgumentException("Sản phẩm không khớp với đơn hàng.");
        }

        if (await _repo.HasReviewedOrderItemAsync(userId, request.OrderItemId))
            throw new InvalidOperationException("Bạn đã đánh giá sản phẩm này rồi.");

        var review = new Review
        {
            ProductId   = request.ProductId,
            UserId      = userId,
            OrderItemId = request.OrderItemId,
            Rating      = request.Rating,
            Comment     = request.Comment,
            ImageUrls   = request.ImageUrls,
            IsApproved  = true,
            CreatedAt   = DateTime.UtcNow
        };

        await _repo.AddAsync(review);

        var user = await _ctx.Users.FindAsync(userId);
        return new ReviewResponse
        {
            ReviewId    = review.ReviewId,
            ProductId   = review.ProductId,
            UserId      = review.UserId,
            UserName    = user?.FullName ?? "",
            UserAvatar  = user?.AvatarUrl,
            OrderItemId = review.OrderItemId,
            Rating      = review.Rating,
            Comment     = review.Comment,
            ImageUrls   = review.ImageUrls,
            IsApproved  = review.IsApproved,
            CreatedAt   = review.CreatedAt
        };
    }

    public async Task<ProductReviewSummary> GetByProductIdAsync(int productId)
    {
        var reviews = await _repo.GetByProductIdAsync(productId);

        return new ProductReviewSummary
        {
            TotalReviews   = reviews.Count,
            AverageRating  = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0,
            Reviews        = reviews.Select(r => new ReviewResponse
            {
                ReviewId    = r.ReviewId,
                ProductId   = r.ProductId,
                UserId      = r.UserId,
                UserName    = r.User?.FullName ?? "",
                UserAvatar  = r.User?.AvatarUrl,
                OrderItemId = r.OrderItemId,
                Rating      = r.Rating,
                Comment     = r.Comment,
                ImageUrls   = r.ImageUrls,
                IsApproved  = r.IsApproved,
                CreatedAt   = r.CreatedAt
            }).ToList()
        };
    }
}

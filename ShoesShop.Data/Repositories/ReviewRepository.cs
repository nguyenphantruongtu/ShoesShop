using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Data.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ShoeStoreDbContext _ctx;
    public ReviewRepository(ShoeStoreDbContext ctx) => _ctx = ctx;

    public async Task<List<Review>> GetByProductIdAsync(int productId)
        => await _ctx.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<Review?> GetByIdAsync(int reviewId)
        => await _ctx.Reviews.Include(r => r.User).FirstOrDefaultAsync(r => r.ReviewId == reviewId);

    public async Task<bool> HasReviewedOrderItemAsync(int userId, int orderItemId)
        => await _ctx.Reviews.AnyAsync(r => r.UserId == userId && r.OrderItemId == orderItemId);

    public async Task AddAsync(Review review)
    {
        await _ctx.Reviews.AddAsync(review);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Review review)
    {
        _ctx.Reviews.Update(review);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(Review review)
    {
        _ctx.Reviews.Remove(review);
        await _ctx.SaveChangesAsync();
    }

    public async Task<(List<Review> Reviews, int TotalCount)> GetForAdminAsync(bool? isApproved, int page, int pageSize)
    {
        var query = _ctx.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
                .ThenInclude(p => p.ProductImages)
            .AsQueryable();

        if (isApproved.HasValue)
            query = query.Where(r => r.IsApproved == isApproved.Value);

        var total = await query.CountAsync();
        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (reviews, total);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using ShoesShop.Shared.DTOs.Review;
using System.Security.Claims;

namespace ShoesShop.API.Controllers;

/// <summary>
/// F11. Review &amp; Rating
/// UC-23: POST /api/reviews              — Customer gửi đánh giá (sau khi Delivered + verified purchase)
///        GET  /api/products/{id}/reviews — Danh sách review công khai của sản phẩm
/// </summary>
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _service;
    public ReviewController(IReviewService service) => _service = service;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── UC-23: Customer gửi đánh giá ────────────────────────────────────────

    /// <summary>
    /// Gửi đánh giá sản phẩm.
    /// Điều kiện: đơn hàng phải ở trạng thái Delivered và chưa đánh giá OrderItem này.
    /// </summary>
    [HttpPost("api/reviews")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(UserId, request);
            return Ok(ApiResponse<ReviewResponse>.Ok(result, "Đánh giá sản phẩm thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ReviewResponse>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<ReviewResponse>.Fail(ex.Message)); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<ReviewResponse>.Fail(ex.Message)); }
    }

    // ── Public: Lấy review của sản phẩm ────────────────────────────────────

    /// <summary>
    /// Danh sách review công khai của sản phẩm (chỉ review IsApproved = true).
    /// Kèm AverageRating và TotalReviews.
    /// </summary>
    [HttpGet("api/products/{productId:int}/reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var result = await _service.GetByProductIdAsync(productId);
        return Ok(ApiResponse<ProductReviewSummary>.Ok(result));
    }
}

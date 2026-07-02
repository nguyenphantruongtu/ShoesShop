using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using ShoesShop.Shared.DTOs.Review;

namespace ShoesShop.API.Controllers;

/// <summary>
/// F11. Review &amp; Rating — UC-24: Admin/Staff kiểm duyệt đánh giá
/// GET   /api/admin/reviews             — Danh sách đánh giá (lọc theo trạng thái duyệt)
/// PATCH /api/admin/reviews/{id}/approve — Duyệt đánh giá → hiển thị công khai
/// PATCH /api/admin/reviews/{id}/reject  — Ẩn đánh giá (huỷ duyệt)
/// DELETE /api/admin/reviews/{id}        — Xóa đánh giá vi phạm/spam
/// </summary>
[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = Roles.AdminOrStaff)]
public class AdminReviewController : ControllerBase
{
    private readonly IReviewService _service;
    public AdminReviewController(IReviewService service) => _service = service;

    /// <summary>status: pending | approved | (bỏ trống = tất cả)</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        bool? isApproved = status?.ToLower() switch
        {
            "pending"  => false,
            "approved" => true,
            _          => null
        };

        var result = await _service.GetForAdminAsync(isApproved, page, pageSize);
        return Ok(ApiResponse<AdminReviewListResponse>.Ok(result));
    }

    [HttpPatch("{reviewId:int}/approve")]
    public async Task<IActionResult> Approve(int reviewId)
    {
        try
        {
            var result = await _service.SetApprovalAsync(reviewId, true);
            return Ok(ApiResponse<ReviewResponse>.Ok(result, "Đã duyệt đánh giá — hiển thị công khai."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ReviewResponse>.Fail(ex.Message)); }
    }

    [HttpPatch("{reviewId:int}/reject")]
    public async Task<IActionResult> Reject(int reviewId)
    {
        try
        {
            var result = await _service.SetApprovalAsync(reviewId, false);
            return Ok(ApiResponse<ReviewResponse>.Ok(result, "Đã ẩn đánh giá."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ReviewResponse>.Fail(ex.Message)); }
    }

    [HttpDelete("{reviewId:int}")]
    public async Task<IActionResult> Delete(int reviewId)
    {
        try
        {
            await _service.DeleteAsync(reviewId);
            return Ok(ApiResponse<object>.Ok(null, "Đã xóa đánh giá."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(ex.Message)); }
    }
}

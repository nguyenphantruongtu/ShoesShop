using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Shared.DTOs.Voucher;

// ── RESPONSE DTOs ──────────────────────────────────────────────────────────────

public class VoucherResponse
{
    public int VoucherId { get; set; }
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public int? UsageLimitPerUser { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Kết quả kiểm tra + tính giảm giá khi khách áp mã voucher ở checkout (chưa ghi DB)</summary>
public class VoucherPreviewResponse
{
    public int VoucherId { get; set; }
    public string Code { get; set; } = null!;
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public decimal DiscountAmount { get; set; }
}

// ── REQUEST DTOs ───────────────────────────────────────────────────────────────

/// <summary>UC-16: Khách nhập mã voucher ở trang giỏ hàng/checkout để xem số tiền được giảm</summary>
public class ApplyVoucherRequest
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal SubTotal { get; set; }
}

public class CreateVoucherRequest
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>Percentage | Fixed</summary>
    [Required]
    public string DiscountType { get; set; } = null!;

    [Range(0.01, double.MaxValue)]
    public decimal DiscountValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinOrderAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxDiscountAmount { get; set; }

    [Range(1, int.MaxValue)]
    public int? UsageLimit { get; set; }

    [Range(1, int.MaxValue)]
    public int? UsageLimitPerUser { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateVoucherRequest
{
    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal DiscountValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinOrderAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxDiscountAmount { get; set; }

    [Range(1, int.MaxValue)]
    public int? UsageLimit { get; set; }

    [Range(1, int.MaxValue)]
    public int? UsageLimitPerUser { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }
}

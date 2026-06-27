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

// ── REQUEST DTOs ───────────────────────────────────────────────────────────────

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

    public decimal? MaxDiscountAmount { get; set; }

    public int? UsageLimit { get; set; }

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

    public decimal? MaxDiscountAmount { get; set; }

    public int? UsageLimit { get; set; }

    public int? UsageLimitPerUser { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }
}

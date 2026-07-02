using ShoesShop.Business.Helpers;
using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;
using ShoesShop.Shared.DTOs.Voucher;

namespace ShoesShop.Business.Services;

public class VoucherService : IVoucherService
{
    private readonly IVoucherRepository _repo;
    public VoucherService(IVoucherRepository repo) => _repo = repo;

    public async Task<List<VoucherResponse>> GetAllAsync()
    {
        var vouchers = await _repo.GetAllAsync();
        return vouchers.Select(MapToResponse).ToList();
    }

    public async Task<VoucherResponse> GetByIdAsync(int voucherId)
    {
        var voucher = await _repo.GetByIdAsync(voucherId)
            ?? throw new KeyNotFoundException($"Không tìm thấy voucher #{voucherId}.");
        return MapToResponse(voucher);
    }

    public async Task<VoucherResponse> CreateAsync(CreateVoucherRequest request)
    {
        var code = request.Code.ToUpper().Trim();

        if (await _repo.CodeExistsAsync(code))
            throw new InvalidOperationException($"Mã voucher '{code}' đã tồn tại.");

        if (request.EndDate <= request.StartDate)
            throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu.");

        if (request.DiscountType != "Percentage" && request.DiscountType != "Fixed")
            throw new ArgumentException("DiscountType phải là 'Percentage' hoặc 'Fixed'.");

        if (request.DiscountType == "Percentage" && request.DiscountValue > 100)
            throw new ArgumentException("Giảm giá theo phần trăm không được vượt quá 100%.");

        var voucher = new Voucher
        {
            Code                = code,
            Description         = request.Description,
            DiscountType        = request.DiscountType,
            DiscountValue       = request.DiscountValue,
            MinOrderAmount      = request.MinOrderAmount,
            MaxDiscountAmount   = request.MaxDiscountAmount,
            UsageLimit          = request.UsageLimit,
            UsageLimitPerUser   = request.UsageLimitPerUser,
            StartDate           = request.StartDate,
            EndDate             = request.EndDate,
            IsActive            = request.IsActive,
            UsedCount           = 0,
            CreatedAt           = DateTime.UtcNow
        };

        await _repo.AddAsync(voucher);
        return MapToResponse(voucher);
    }

    public async Task<VoucherResponse> UpdateAsync(int voucherId, UpdateVoucherRequest request)
    {
        var voucher = await _repo.GetByIdAsync(voucherId)
            ?? throw new KeyNotFoundException($"Không tìm thấy voucher #{voucherId}.");

        if (request.EndDate <= request.StartDate)
            throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu.");

        if (voucher.DiscountType == "Percentage" && request.DiscountValue > 100)
            throw new ArgumentException("Giảm giá theo phần trăm không được vượt quá 100%.");

        voucher.Description       = request.Description;
        voucher.DiscountValue     = request.DiscountValue;
        voucher.MinOrderAmount    = request.MinOrderAmount;
        voucher.MaxDiscountAmount = request.MaxDiscountAmount;
        voucher.UsageLimit        = request.UsageLimit;
        voucher.UsageLimitPerUser = request.UsageLimitPerUser;
        voucher.StartDate         = request.StartDate;
        voucher.EndDate           = request.EndDate;
        voucher.IsActive          = request.IsActive;

        await _repo.UpdateAsync(voucher);
        return MapToResponse(voucher);
    }

    public async Task DeleteAsync(int voucherId)
    {
        var voucher = await _repo.GetByIdAsync(voucherId)
            ?? throw new KeyNotFoundException($"Không tìm thấy voucher #{voucherId}.");

        if (voucher.UsedCount > 0)
            throw new InvalidOperationException("Không thể xóa voucher đã được sử dụng.");

        await _repo.DeleteAsync(voucher);
    }

    public async Task<VoucherPreviewResponse> ValidateForOrderAsync(string code, decimal subTotal, int userId)
    {
        var voucher = await _repo.GetByCodeAsync(code.Trim())
            ?? throw new KeyNotFoundException($"Mã voucher '{code}' không tồn tại.");

        var userUsage = await _repo.CountUserUsageAsync(userId, voucher.VoucherId);
        var discount  = VoucherValidator.Validate(voucher, subTotal, userUsage);

        return new VoucherPreviewResponse
        {
            VoucherId      = voucher.VoucherId,
            Code           = voucher.Code,
            DiscountType   = voucher.DiscountType,
            DiscountValue  = voucher.DiscountValue,
            DiscountAmount = discount
        };
    }

    private static VoucherResponse MapToResponse(Voucher v) => new()
    {
        VoucherId         = v.VoucherId,
        Code              = v.Code,
        Description       = v.Description,
        DiscountType      = v.DiscountType,
        DiscountValue     = v.DiscountValue,
        MinOrderAmount    = v.MinOrderAmount,
        MaxDiscountAmount = v.MaxDiscountAmount,
        UsageLimit        = v.UsageLimit,
        UsedCount         = v.UsedCount,
        UsageLimitPerUser = v.UsageLimitPerUser,
        StartDate         = v.StartDate,
        EndDate           = v.EndDate,
        IsActive          = v.IsActive,
        CreatedAt         = v.CreatedAt
    };
}

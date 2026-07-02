using ShoesShop.Data.Entities;

namespace ShoesShop.Business.Helpers;

/// <summary>
/// Quy tắc áp dụng voucher — dùng chung cho preview lúc checkout (VoucherService)
/// và lúc tạo đơn thật (OrderService) để đảm bảo nhất quán.
/// </summary>
public static class VoucherValidator
{
    /// <summary>Kiểm tra voucher hợp lệ với đơn hàng hiện tại và trả về số tiền được giảm.</summary>
    public static decimal Validate(Voucher voucher, decimal subTotal, int userUsageCount)
    {
        if (!voucher.IsActive)
            throw new InvalidOperationException("Voucher không còn hiệu lực.");

        var now = DateTime.UtcNow;
        if (now < voucher.StartDate)
            throw new InvalidOperationException("Voucher chưa đến ngày áp dụng.");
        if (now > voucher.EndDate)
            throw new InvalidOperationException("Voucher đã hết hạn.");

        if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit.Value)
            throw new InvalidOperationException("Voucher đã hết lượt sử dụng.");

        if (voucher.UsageLimitPerUser.HasValue && userUsageCount >= voucher.UsageLimitPerUser.Value)
            throw new InvalidOperationException("Bạn đã dùng hết số lần cho phép với voucher này.");

        if (subTotal < voucher.MinOrderAmount)
            throw new InvalidOperationException(
                $"Đơn hàng cần tối thiểu {voucher.MinOrderAmount:N0}₫ để áp dụng voucher này.");

        var discount = voucher.DiscountType == "Percentage"
            ? Math.Round(subTotal * voucher.DiscountValue / 100m, 0)
            : voucher.DiscountValue;

        if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
            discount = voucher.MaxDiscountAmount.Value;

        if (discount > subTotal)
            discount = subTotal;

        return discount;
    }
}

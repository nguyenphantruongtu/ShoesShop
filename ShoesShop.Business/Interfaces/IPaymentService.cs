using ShoesShop.Shared.DTOs.Payment;

namespace ShoesShop.Business.Interfaces;

public interface IPaymentService
{
    /// <summary>UC-18: Tạo PayOS payment link từ orderId, trả về checkoutUrl</summary>
    Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(int orderId, int userId);

    /// <summary>UC-43: Xử lý webhook từ PayOS, verify signature, update PaymentStatus</summary>
    Task<bool> HandleWebhookAsync(PaymentWebhookRequest webhookBody);

    /// <summary>Lấy trạng thái thanh toán (Customer — verify ownership)</summary>
    Task<PaymentStatusResponse> GetPaymentStatusAsync(int orderId, int userId);

    /// <summary>Lấy trạng thái thanh toán (Admin/Staff — no ownership check)</summary>
    Task<PaymentStatusResponse> GetPaymentStatusAdminAsync(int orderId);

    /// <summary>Đồng bộ trạng thái thanh toán từ PayOS (dùng khi webhook không về được)</summary>
    Task SyncPaymentStatusAsync(int orderId);
}

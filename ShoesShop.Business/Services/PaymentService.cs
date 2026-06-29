using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.Webhooks;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Shared.Constants;
using ShoesShop.Shared.DTOs.Payment;
using PayOSPaymentLinkRequest = PayOS.Models.V2.PaymentRequests.CreatePaymentLinkRequest;
using PayOSPaymentLinkItem = PayOS.Models.V2.PaymentRequests.PaymentLinkItem;

namespace ShoesShop.Business.Services;

public class PaymentService : IPaymentService
{
    private readonly PayOSClient _payOS;
    private readonly ShoeStoreDbContext _context;
    private readonly IConfiguration _config;

    public PaymentService(PayOSClient payOS, ShoeStoreDbContext context, IConfiguration config)
    {
        _payOS = payOS;
        _context = context;
        _config = config;
    }

    // ── UC-18: Tạo payment link ────────────────────────────────────────────────
    public async Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(int orderId, int userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");

        if (order.PaymentStatus == PaymentStatus.Paid)
            throw new InvalidOperationException("Đơn hàng đã được thanh toán rồi.");

        var payosMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(m => m.MethodName == "PayOS" && m.IsActive)
            ?? throw new InvalidOperationException("Phương thức thanh toán PayOS không khả dụng.");

        var webBaseUrl = _config["AppSettings:WebBaseUrl"] ?? "https://localhost:7161";

        var items = order.OrderItems.Select(i =>
        {
            var name = i.ProductName.Length > 25 ? i.ProductName[..25] : i.ProductName;
            return new PayOSPaymentLinkItem { Name = name, Quantity = i.Quantity, Price = (long)i.UnitPrice };
        }).ToList();

        var description = $"Don hang #{order.OrderCode}";
        if (description.Length > 25) description = description[..25];

        var paymentRequest = new PayOSPaymentLinkRequest
        {
            OrderCode   = orderId,
            Amount      = (long)order.TotalAmount,
            Description = description,
            Items       = items,
            ReturnUrl   = $"{webBaseUrl}/Payment/Success?orderId={orderId}",
            CancelUrl   = $"{webBaseUrl}/Payment/Cancel?orderId={orderId}"
        };

        // PayOSClient.PaymentRequests (không phải V2.PaymentRequests)
        var result = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

        // Idempotent: tạo hoặc cập nhật Payment record
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
        if (payment == null)
        {
            _context.Payments.Add(new Payment
            {
                OrderId         = orderId,
                PaymentMethodId = payosMethod.PaymentMethodId,
                Amount          = order.TotalAmount,
                Status          = "Pending",
                GatewayResponse = result.PaymentLinkId,
                CreatedAt       = DateTime.UtcNow
            });
        }
        else
        {
            payment.GatewayResponse = result.PaymentLinkId;
            payment.Status          = "Pending";
        }

        await _context.SaveChangesAsync();

        return new CreatePaymentLinkResponse
        {
            OrderId       = orderId,
            CheckoutUrl   = result.CheckoutUrl,
            QrCode        = result.QrCode,
            PaymentLinkId = result.PaymentLinkId
        };
    }

    // ── UC-43: Xử lý webhook (idempotent) ────────────────────────────────────
    public async Task<bool> HandleWebhookAsync(PaymentWebhookRequest body)
    {
        try
        {
            // Map sang Webhook của SDK (Desc → Description, không có Desc field)
            var webhook = new Webhook
            {
                Code        = body.Code,
                Description = body.Desc,
                Success     = body.Success,
                Signature   = body.Signature,
                Data = body.Data == null ? null : new WebhookData
                {
                    OrderCode           = body.Data.OrderCode,
                    Amount              = body.Data.Amount,
                    Description         = body.Data.Description,
                    AccountNumber       = body.Data.AccountNumber,
                    Reference           = body.Data.Reference,
                    TransactionDateTime = body.Data.TransactionDateTime,
                    Currency            = body.Data.Currency,
                    PaymentLinkId       = body.Data.PaymentLinkId,
                    Code                = body.Data.Code,
                    Description2        = body.Data.Desc
                }
            };

            // Verify signature — ném exception nếu không hợp lệ
            var verifiedData = await _payOS.Webhooks.VerifyAsync(webhook);

            int orderId = (int)verifiedData.OrderCode;

            var order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return true; // idempotent, bỏ qua nếu không tìm thấy

            // Idempotent: bỏ qua nếu đã xử lý
            if (order.PaymentStatus == PaymentStatus.Paid) return true;

            var payment = order.Payments.FirstOrDefault();

            if (verifiedData.Code == "00") // success
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.UpdatedAt     = DateTime.UtcNow;

                if (payment != null)
                {
                    payment.Status          = "Paid";
                    payment.TransactionCode = verifiedData.Reference;
                    payment.PaidAt          = DateTime.UtcNow;
                    payment.GatewayResponse = verifiedData.PaymentLinkId;
                }
            }
            else // cancelled / failed
            {
                if (payment != null)
                    payment.Status = "Cancelled";
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ── Trạng thái thanh toán (Customer) ─────────────────────────────────────
    public async Task<PaymentStatusResponse> GetPaymentStatusAsync(int orderId, int userId)
    {
        var order = await _context.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");

        if (order.PaymentStatus != PaymentStatus.Paid)
            await SyncFromPayOSAsync(order);

        var payment = order.Payments.FirstOrDefault();
        return new PaymentStatusResponse
        {
            OrderId         = orderId,
            PaymentStatus   = order.PaymentStatus,
            TransactionCode = payment?.TransactionCode,
            PaidAt          = payment?.PaidAt
        };
    }

    // ── Trạng thái thanh toán (Admin/Staff) ──────────────────────────────────
    public async Task<PaymentStatusResponse> GetPaymentStatusAdminAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderId == orderId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");

        if (order.PaymentStatus != PaymentStatus.Paid)
            await SyncFromPayOSAsync(order);

        var payment = order.Payments.FirstOrDefault();
        return new PaymentStatusResponse
        {
            OrderId         = orderId,
            PaymentStatus   = order.PaymentStatus,
            TransactionCode = payment?.TransactionCode,
            PaidAt          = payment?.PaidAt
        };
    }

    // ── Public sync (gọi từ Order controllers) ───────────────────────────────
    public async Task SyncPaymentStatusAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order == null || order.PaymentStatus == PaymentStatus.Paid) return;
        await SyncFromPayOSAsync(order);
    }

    // ── Đồng bộ trạng thái từ PayOS API (fallback khi webhook không về) ──────
    private async Task SyncFromPayOSAsync(Order order)
    {
        try
        {
            var link = await _payOS.PaymentRequests.GetAsync((long)order.OrderId);
            if (link == null) return;
            if (!link.Status.ToString().Equals("PAID", StringComparison.OrdinalIgnoreCase))
                return;

            order.PaymentStatus = PaymentStatus.Paid;
            order.UpdatedAt     = DateTime.UtcNow;

            var payment = order.Payments.FirstOrDefault();
            if (payment != null)
            {
                payment.Status          = "Paid";
                payment.PaidAt          = DateTime.UtcNow;
                payment.TransactionCode ??= link.Transactions?.FirstOrDefault()?.Reference;
            }

            await _context.SaveChangesAsync();
        }
        catch { /* bỏ qua lỗi network/PayOS */ }
    }
}

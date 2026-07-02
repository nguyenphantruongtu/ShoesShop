using ShoesShop.Business.Helpers;
using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;
using ShoesShop.Shared.DTOs;
using ShoesShop.Shared.DTOs.Order;

namespace ShoesShop.Business.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;
    private readonly IVoucherRepository _voucherRepo;

    public OrderService(IOrderRepository repo, IVoucherRepository voucherRepo)
    {
        _repo        = repo;
        _voucherRepo = voucherRepo;
    }

    // ── UC-06: Tạo đơn hàng từ Cart ─────────────────────────────────────────
    public async Task<int> CreateOrderAsync(CreateOrderRequest request, int userId)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new ArgumentException("Giỏ hàng trống.");

        await _repo.BeginTransactionAsync();
        try
        {
            var orderCode = "HD" + DateTime.UtcNow.ToString("yyMMdd") + Random.Shared.Next(100, 999);

            var order = new Order
            {
                OrderCode       = orderCode,
                UserId          = userId,
                RecipientName   = request.RecipientName,
                RecipientPhone  = request.RecipientPhone,
                ShippingAddress = request.ShippingAddress,
                Province        = request.Province,
                District        = request.District,
                Ward            = request.Ward,
                Note            = request.Note,
                OrderStatus     = OrderStatus.Pending,
                PaymentStatus   = PaymentStatus.Unpaid,
                CreatedAt       = DateTime.UtcNow
            };

            await _repo.AddAsync(order);
            await _repo.SaveChangesAsync();

            // Server là nguồn sự thật về giá — KHÔNG tin giá/tổng tiền client gửi lên.
            decimal subTotal = 0m;

            foreach (var item in request.Items)
            {
                if (item.Quantity < 1)
                    throw new ArgumentException("Số lượng mỗi sản phẩm phải ít nhất là 1.");

                var variant = await _repo.GetVariantByIdAsync(item.VariantId)
                    ?? throw new KeyNotFoundException($"Không tìm thấy variant #{item.VariantId}.");

                if (!variant.IsActive || !variant.Product.IsActive)
                    throw new InvalidOperationException(
                        $"Sản phẩm '{variant.Product.ProductName}' không còn được bán.");

                // Chỉ kiểm tra đủ hàng — CHƯA trừ kho ở bước này. Tồn kho chỉ thực sự
                // bị trừ khi staff xác nhận đơn (xem ConfirmOrderAsync).
                if (variant.StockQuantity < item.Quantity)
                    throw new InvalidOperationException(
                        $"'{variant.Product.ProductName}' không đủ hàng (tồn: {variant.StockQuantity}, yêu cầu: {item.Quantity}).");

                // Giá lấy từ DB theo thứ tự ưu tiên: giá variant → giá sale → giá gốc
                var unitPrice = variant.Price ?? variant.Product.SalePrice ?? variant.Product.BasePrice;
                var lineTotal = unitPrice * item.Quantity;
                subTotal += lineTotal;

                await _repo.AddAsync(new OrderItem
                {
                    OrderId     = order.OrderId,
                    VariantId   = item.VariantId,
                    ProductName = variant.Product.ProductName,
                    SizeValue   = variant.Size.SizeValue,
                    ColorName   = variant.Color.ColorName,
                    UnitPrice   = unitPrice,
                    Quantity    = item.Quantity,
                    LineTotal   = lineTotal
                });
            }

            // Voucher: server tự validate lại mã + tính giảm giá — không tin DiscountAmount client gửi.
            Voucher? voucher = null;
            var discountAmount = 0m;
            if (!string.IsNullOrWhiteSpace(request.VoucherCode))
            {
                voucher = await _voucherRepo.GetByCodeAsync(request.VoucherCode.Trim())
                    ?? throw new KeyNotFoundException($"Mã voucher '{request.VoucherCode}' không tồn tại.");

                var userUsage = await _voucherRepo.CountUserUsageAsync(userId, voucher.VoucherId);
                discountAmount = VoucherValidator.Validate(voucher, subTotal, userUsage);
            }

            // Tổng tiền tính lại ở server (không tính phí giao hàng — đã bỏ khỏi scope).
            var totalAmount = subTotal - discountAmount;

            order.SubTotal       = subTotal;
            order.DiscountAmount = discountAmount;
            order.TotalAmount    = totalAmount;
            order.VoucherId      = voucher?.VoucherId;
            await _repo.UpdateAsync(order);

            if (voucher != null)
            {
                voucher.UsedCount += 1;
                await _voucherRepo.UpdateAsync(voucher);
            }

            // Ghi nhận phương thức thanh toán khách chọn (COD / PayOS)
            var method = await _repo.GetPaymentMethodByNameAsync(request.PaymentMethod)
                         ?? await _repo.GetPaymentMethodByNameAsync("COD");
            if (method != null)
            {
                await _repo.AddAsync(new Payment
                {
                    OrderId         = order.OrderId,
                    PaymentMethodId = method.PaymentMethodId,
                    Amount          = order.TotalAmount,
                    Status          = "Pending",
                    CreatedAt       = DateTime.UtcNow
                });
            }

            await _repo.SaveChangesAsync();
            await _repo.CommitTransactionAsync();

            return order.OrderId;
        }
        catch
        {
            await _repo.RollbackTransactionAsync();
            throw;
        }
    }

    // ── UC-31: List đơn hàng cho Staff ──────────────────────────────────────
    public async Task<OrderListResponse> GetOrdersAsync(
        string? search, string? status, int page, int pageSize)
    {
        var (orders, total) = await _repo.GetPaginatedAsync(search, status, page, pageSize);

        return new OrderListResponse
        {
            Orders = orders.Select(o => new OrderListItem
            {
                OrderId        = o.OrderId,
                OrderCode      = o.OrderCode,
                CustomerName   = o.RecipientName,
                CustomerPhone  = o.RecipientPhone,
                TotalAmount    = o.TotalAmount,
                OrderStatus    = o.OrderStatus,
                PaymentStatus  = o.PaymentStatus,
                ItemCount      = o.OrderItems.Count,
                CreatedAt      = o.CreatedAt,
                HandledByStaff = o.HandledByStaff?.FullName,
                Items          = MapItemPreviews(o)
            }).ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    // ── UC-32: Chi tiết đơn hàng ────────────────────────────────────────────
    public async Task<OrderDetailResponse> GetOrderDetailAsync(int orderId)
    {
        var order = await _repo.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");
        return MapToDetail(order);
    }

    // ── UC-32: Xác nhận đơn (Pending → Confirmed) + TRỪ KHO ─────────────────
    public async Task<OrderDetailResponse> ConfirmOrderAsync(int orderId, int staffId)
    {
        var order = await _repo.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");

        if (order.OrderStatus != OrderStatus.Pending)
            throw new InvalidOperationException(
                $"Chỉ có thể xác nhận đơn ở trạng thái Pending. Hiện tại: {order.OrderStatus}.");

        await _repo.BeginTransactionAsync();
        try
        {
            // Đây là bước tồn kho thực sự bị trừ. Kiểm tra lại tồn vì có thể đã thay
            // đổi kể từ lúc khách đặt (nhiều đơn Pending cùng giành một mặt hàng).
            await DeductStockForOrderAsync(order);

            order.OrderStatus      = OrderStatus.Confirmed;
            order.HandledByStaffId = staffId;
            order.UpdatedAt        = DateTime.UtcNow;

            await _repo.UpdateAsync(order);
            await _repo.AddStatusHistoryAsync(new OrderStatusHistory
            {
                OrderId         = orderId,
                Status          = OrderStatus.Confirmed,
                Note            = "Staff xác nhận đơn hàng + trừ tồn kho.",
                ChangedByUserId = staffId,
                ChangedAt       = DateTime.UtcNow
            });

            await _repo.CommitTransactionAsync();
        }
        catch
        {
            await _repo.RollbackTransactionAsync();
            throw;
        }

        var updated = await _repo.GetByIdWithDetailsAsync(orderId);
        return MapToDetail(updated!);
    }

    // ── UC-33 + UC-34: Cập nhật trạng thái ─────────────────────────────────
    public async Task<OrderDetailResponse> UpdateStatusAsync(
        int orderId, UpdateOrderStatusRequest request, int staffId)
    {
        var order = await _repo.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");

        var newStatus = request.NewStatus;

        if (!OrderStatus.IsValidTransition(order.OrderStatus, newStatus))
            throw new InvalidOperationException(
                $"Không thể chuyển từ '{order.OrderStatus}' sang '{newStatus}'.");

        // Khi giao hàng thành công: đơn COD được thu tiền → đánh dấu đã thanh toán.
        // (Đơn PayOS đã Paid từ lúc checkout nên điều kiện này bỏ qua.)
        if (newStatus == OrderStatus.Delivered && order.PaymentStatus != PaymentStatus.Paid)
        {
            order.PaymentStatus = PaymentStatus.Paid;

            var payment = await _repo.GetPaymentByOrderIdAsync(orderId);
            if (payment != null)
            {
                payment.Status = "Paid";
                payment.PaidAt = DateTime.UtcNow;
                await _repo.UpdatePaymentAsync(payment);
            }
        }

        order.OrderStatus      = newStatus;
        order.HandledByStaffId = staffId;
        order.UpdatedAt        = DateTime.UtcNow;

        await _repo.UpdateAsync(order);
        await _repo.AddStatusHistoryAsync(new OrderStatusHistory
        {
            OrderId         = orderId,
            Status          = newStatus,
            Note            = request.Note ?? $"Chuyển trạng thái sang {newStatus}.",
            ChangedByUserId = staffId,
            ChangedAt       = DateTime.UtcNow
        });

        var updated = await _repo.GetByIdWithDetailsAsync(orderId);
        return MapToDetail(updated!);
    }

    // ── UC-35: Hủy đơn + rollback stock ────────────────────────────────────
    public async Task<OrderDetailResponse> CancelOrderAsync(
        int orderId, CancelOrderRequest request, int staffId)
    {
        var order = await _repo.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");

        if (!OrderStatus.IsCancellable(order.OrderStatus))
            throw new InvalidOperationException(
                $"Không thể hủy đơn ở trạng thái '{order.OrderStatus}'.");

        // Chỉ hoàn kho nếu đơn đã qua bước trừ kho (từ Confirmed trở đi).
        // Đơn Pending chưa trừ kho nên không cần hoàn.
        if (OrderStatus.IsStockDeducted(order.OrderStatus))
            await RestoreStockForOrderAsync(order);

        await RollbackVoucherUsageAsync(order);

        order.OrderStatus      = OrderStatus.Cancelled;
        order.CancelReason     = request.CancelReason;
        order.CancelledAt      = DateTime.UtcNow;
        order.HandledByStaffId = staffId;
        order.UpdatedAt        = DateTime.UtcNow;

        await _repo.UpdateAsync(order);
        await _repo.AddStatusHistoryAsync(new OrderStatusHistory
        {
            OrderId         = orderId,
            Status          = OrderStatus.Cancelled,
            Note            = $"Hủy bởi Staff. Lý do: {request.CancelReason}",
            ChangedByUserId = staffId,
            ChangedAt       = DateTime.UtcNow
        });

        var updated = await _repo.GetByIdWithDetailsAsync(orderId);
        return MapToDetail(updated!);
    }

    // ── UC-19: Lịch sử đơn hàng của Customer ───────────────────────────────
    public async Task<OrderListResponse> GetMyOrdersAsync(
        int userId, string? status, int page, int pageSize)
    {
        var (orders, total) = await _repo.GetByUserIdAsync(userId, status, page, pageSize);

        return new OrderListResponse
        {
            Orders = orders.Select(o => new OrderListItem
            {
                OrderId       = o.OrderId,
                OrderCode     = o.OrderCode,
                CustomerName  = o.RecipientName,
                CustomerPhone = o.RecipientPhone,
                TotalAmount   = o.TotalAmount,
                OrderStatus   = o.OrderStatus,
                PaymentStatus = o.PaymentStatus,
                ItemCount     = o.OrderItems.Count,
                CreatedAt     = o.CreatedAt,
                Items         = MapItemPreviews(o)
            }).ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    // ── UC-20 + UC-22: Chi tiết đơn hàng của Customer ──────────────────────
    public async Task<OrderDetailResponse> GetMyOrderDetailAsync(int orderId, int userId)
    {
        var order = await _repo.GetByIdAndUserIdAsync(orderId, userId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");
        return MapToDetail(order);
    }

    // ── UC-21: Customer tự hủy đơn ─────────────────────────────────────────
    public async Task<OrderDetailResponse> CancelByCustomerAsync(int orderId, int userId, string cancelReason)
    {
        var order = await _repo.GetByIdAndUserIdAsync(orderId, userId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");

        if (!OrderStatus.IsCancellable(order.OrderStatus))
            throw new InvalidOperationException(
                $"Không thể hủy đơn ở trạng thái '{order.OrderStatus}'.");

        // Chỉ hoàn kho nếu đơn đã qua bước trừ kho (từ Confirmed trở đi).
        if (OrderStatus.IsStockDeducted(order.OrderStatus))
            await RestoreStockForOrderAsync(order);

        await RollbackVoucherUsageAsync(order);

        order.OrderStatus  = OrderStatus.Cancelled;
        order.CancelReason = cancelReason;
        order.CancelledAt  = DateTime.UtcNow;
        order.UpdatedAt    = DateTime.UtcNow;

        await _repo.UpdateAsync(order);
        await _repo.AddStatusHistoryAsync(new OrderStatusHistory
        {
            OrderId         = orderId,
            Status          = OrderStatus.Cancelled,
            Note            = $"Khách hàng hủy đơn. Lý do: {cancelReason}",
            ChangedByUserId = userId,
            ChangedAt       = DateTime.UtcNow
        });

        var updated = await _repo.GetByIdAndUserIdAsync(orderId, userId);
        return MapToDetail(updated!);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Trừ tồn kho cho toàn bộ item của đơn — kiểm tra lại đủ hàng, ném lỗi nếu thiếu.</summary>
    private async Task DeductStockForOrderAsync(Order order)
    {
        foreach (var item in order.OrderItems)
        {
            var variant = await _repo.GetVariantByIdAsync(item.VariantId)
                ?? throw new KeyNotFoundException($"Không tìm thấy variant #{item.VariantId}.");

            if (variant.StockQuantity < item.Quantity)
                throw new InvalidOperationException(
                    $"'{variant.Product.ProductName}' không đủ hàng để xác nhận (tồn: {variant.StockQuantity}, cần: {item.Quantity}). Vui lòng nhập thêm hàng hoặc hủy đơn.");

            variant.StockQuantity -= item.Quantity;
            await _repo.UpdateVariantAsync(variant);
        }
    }

    /// <summary>Hoàn tồn kho cho toàn bộ item của đơn (khi hủy đơn đã trừ kho).</summary>
    private async Task RestoreStockForOrderAsync(Order order)
    {
        foreach (var item in order.OrderItems)
        {
            var variant = await _repo.GetVariantByIdAsync(item.VariantId);
            if (variant != null)
            {
                variant.StockQuantity += item.Quantity;
                await _repo.UpdateVariantAsync(variant);
            }
        }
    }

    private async Task RollbackVoucherUsageAsync(Order order)
    {
        if (!order.VoucherId.HasValue) return;

        var voucher = await _voucherRepo.GetByIdAsync(order.VoucherId.Value);
        if (voucher != null && voucher.UsedCount > 0)
        {
            voucher.UsedCount -= 1;
            await _voucherRepo.UpdateAsync(voucher);
        }
    }

    /// <summary>Lấy ảnh đại diện + tối đa 4 sản phẩm đầu để hiển thị thu nhỏ (Shopee-style) ở danh sách đơn hàng.</summary>
    private static List<OrderItemPreview> MapItemPreviews(Order o) =>
        o.OrderItems.Take(4).Select(i => new OrderItemPreview
        {
            ProductName = i.ProductName,
            ImageUrl    = GetPrimaryImageUrl(i),
            SizeValue   = i.SizeValue,
            ColorName   = i.ColorName,
            Quantity    = i.Quantity
        }).ToList();

    private static string? GetPrimaryImageUrl(OrderItem i) =>
        i.Variant?.Product?.ProductImages
            .OrderByDescending(img => img.IsPrimary)
            .Select(img => img.ImageUrl)
            .FirstOrDefault();

    private static OrderDetailResponse MapToDetail(Order o) => new()
    {
        OrderId         = o.OrderId,
        OrderCode       = o.OrderCode,
        CustomerId      = o.UserId,
        CustomerName    = o.User.FullName,
        CustomerEmail   = o.User.Email,
        RecipientName   = o.RecipientName,
        RecipientPhone  = o.RecipientPhone,
        ShippingAddress = o.ShippingAddress,
        Province        = o.Province,
        District        = o.District,
        Ward            = o.Ward,
        SubTotal        = o.SubTotal,
        DiscountAmount  = o.DiscountAmount,
        TotalAmount     = o.TotalAmount,
        VoucherCode     = o.Voucher?.Code,
        OrderStatus     = o.OrderStatus,
        PaymentStatus   = o.PaymentStatus,
        Note            = o.Note,
        CancelReason    = o.CancelReason,
        CancelledAt     = o.CancelledAt,
        HandledByStaff  = o.HandledByStaff?.FullName,
        CreatedAt       = o.CreatedAt,
        UpdatedAt       = o.UpdatedAt,

        Items = o.OrderItems.Select(i => new OrderItemResponse
        {
            OrderItemId     = i.OrderItemId,
            VariantId       = i.VariantId,
            ProductName     = i.ProductName,
            PrimaryImageUrl = GetPrimaryImageUrl(i),
            SizeValue       = i.SizeValue,
            ColorName       = i.ColorName,
            Sku             = i.Variant?.Sku,
            UnitPrice       = i.UnitPrice,
            Quantity        = i.Quantity,
            LineTotal       = i.LineTotal,
            ProductId       = i.Variant?.ProductId ?? 0,
            IsReviewed  = i.Reviews != null && i.Reviews.Any()
        }).ToList(),

        StatusHistory = o.OrderStatusHistories.Select(h => new OrderStatusHistoryResponse
        {
            HistoryId = h.HistoryId,
            Status    = h.Status,
            Note      = h.Note,
            ChangedBy = h.ChangedByUser?.FullName,
            ChangedAt = h.ChangedAt
        }).ToList()
    };
}
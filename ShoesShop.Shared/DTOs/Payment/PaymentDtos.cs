namespace ShoesShop.Shared.DTOs.Payment;

public class CreatePaymentLinkRequest
{
    public int OrderId { get; set; }
}

public class CreatePaymentLinkResponse
{
    public int OrderId { get; set; }
    public string CheckoutUrl { get; set; } = null!;
    public string? QrCode { get; set; }
    public string? PaymentLinkId { get; set; }
}

public class PaymentStatusResponse
{
    public int OrderId { get; set; }
    public string PaymentStatus { get; set; } = null!;
    public string? TransactionCode { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class PaymentWebhookRequest
{
    public string? Code { get; set; }
    public string? Desc { get; set; }
    public bool Success { get; set; }
    public PaymentWebhookData? Data { get; set; }
    public string? Signature { get; set; }
}

public class PaymentWebhookData
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string? Description { get; set; }
    public string? AccountNumber { get; set; }
    public string? Reference { get; set; }
    public string? TransactionDateTime { get; set; }
    public string? Currency { get; set; }
    public string? PaymentLinkId { get; set; }
    public string? Code { get; set; }
    public string? Desc { get; set; }
}

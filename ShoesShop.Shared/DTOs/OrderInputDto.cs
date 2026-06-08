using System.Collections.Generic;

namespace ShoesShop.Shared.DTOs;

public class OrderInputDto
{
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string Province { get; set; } = "Chưa phân loại";
    public string District { get; set; } = "Chưa phân loại";
    public string Ward { get; set; } = "Chưa phân loại";
    public string? Note { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemInputDto> OrderItems { get; set; } = new();
}

public class OrderItemInputDto
{
    public int VariantId { get; set; }    
    public string ProductName { get; set; } = string.Empty;
    public string SizeValue { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } 
}
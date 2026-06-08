namespace ShoesShop.Shared.DTOs.Cart;

public class AddToCartRequest
{
    public int VariantId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}

public class CartItemResponse
{
    public int CartItemId   { get; set; }
    public int VariantId    { get; set; }
    public int ProductId    { get; set; }
    public string ProductName { get; set; } = null!;
    public string Slug        { get; set; } = null!;
    public string SizeName    { get; set; } = null!;
    public string ColorName   { get; set; } = null!;
    public string? ImageUrl   { get; set; }
    public decimal UnitPrice  { get; set; }
    public int Quantity       { get; set; }
    public int StockQuantity  { get; set; }
    public decimal SubTotal   { get; set; }
}

public class CartResponse
{
    public int CartId                    { get; set; }
    public List<CartItemResponse> Items  { get; set; } = new();
    public int TotalItems                { get; set; }
    public decimal TotalAmount           { get; set; }
}

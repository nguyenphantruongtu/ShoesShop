namespace ShoesShop.Shared.DTOs.Address;

public class AddressResponse
{
    public int AddressId { get; set; }
    public string RecipientName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string District { get; set; } = null!;
    public string Ward { get; set; } = null!;
    public string StreetAddress { get; set; } = null!;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Full address string for display</summary>
    public string FullAddress => $"{StreetAddress}, {Ward}, {District}, {Province}";
}

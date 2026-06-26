namespace ShoesShop.Shared.DTOs;

public class ExportTypeDto
{
    public string Key { get; set; } = null!;
    public string Label { get; set; } = null!;
    public bool SupportsDateRange { get; set; }
    public bool SupportsTop { get; set; }
}

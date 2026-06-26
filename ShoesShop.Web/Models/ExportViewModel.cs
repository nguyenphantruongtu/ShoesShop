using ShoesShop.Shared.DTOs;

namespace ShoesShop.Web.Models;

public class ExportViewModel
{
    public string SelectedType { get; set; } = "products";
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Top { get; set; } = 20;
    public IReadOnlyList<ExportTypeDto> ExportTypes { get; set; } = Array.Empty<ExportTypeDto>();
    public string? ErrorMessage { get; set; }
}

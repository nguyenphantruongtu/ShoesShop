using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Shared.DTOs.Catalog;

// --- SIZE ---
public class CreateSizeRequest
{
    [Required][MaxLength(20)] public string SizeValue { get; set; } = null!;
    public int DisplayOrder { get; set; } = 0;
}

public class UpdateSizeRequest
{
    [Required][MaxLength(20)] public string SizeValue { get; set; } = null!;
    public int DisplayOrder { get; set; }
}

public class SizeResponse
{
    public int SizeId { get; set; }
    public string SizeValue { get; set; } = null!;
    public int DisplayOrder { get; set; }
}

// --- COLOR ---
public class CreateColorRequest
{
    [Required][MaxLength(50)] public string ColorName { get; set; } = null!;
    [MaxLength(7)] public string? HexCode { get; set; }   // e.g. #FF5733
}

public class UpdateColorRequest
{
    [Required][MaxLength(50)] public string ColorName { get; set; } = null!;
    [MaxLength(7)] public string? HexCode { get; set; }
}

public class ColorResponse
{
    public int ColorId { get; set; }
    public string ColorName { get; set; } = null!;
    public string? HexCode { get; set; }
}

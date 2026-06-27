using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Shared.DTOs.Catalog;

public class CreateBrandRequest
{
    [Required][MaxLength(100)] public string BrandName { get; set; } = null!;
    [MaxLength(500)] public string? LogoUrl { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateBrandRequest
{
    [Required][MaxLength(100)] public string BrandName { get; set; } = null!;
    [MaxLength(500)] public string? LogoUrl { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class BrandResponse
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Shared.DTOs.Catalog;

public class CreateCategoryRequest
{
    [Required][MaxLength(100)] public string CategoryName { get; set; } = null!;
    [MaxLength(120)] public string? Slug { get; set; }          // auto-gen if empty
    public int? ParentCategoryId { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    [MaxLength(500)] public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCategoryRequest
{
    [Required][MaxLength(100)] public string CategoryName { get; set; } = null!;
    [MaxLength(120)] public string? Slug { get; set; }
    public int? ParentCategoryId { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    [MaxLength(500)] public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}

public class CategoryResponse
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CategoryResponse> Children { get; set; } = new();
}

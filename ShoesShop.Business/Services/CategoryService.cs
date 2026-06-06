using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Business.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;
    public CategoryService(ICategoryRepository repo) => _repo = repo;

    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        var all = await _repo.GetAllAsync();
        // Build tree: lấy root trước, gắn children
        var lookup = all.ToDictionary(c => c.CategoryId, c => MapToResponse(c));
        foreach (var cat in all.Where(c => c.ParentCategoryId.HasValue))
        {
            if (lookup.TryGetValue(cat.ParentCategoryId!.Value, out var parent))
                parent.Children.Add(lookup[cat.CategoryId]);
        }
        return lookup.Values.Where(c => c.ParentCategoryId == null).ToList();
    }

    public async Task<CategoryResponse> GetByIdAsync(int id)
    {
        var cat = await _repo.GetByIdWithChildrenAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy danh mục.");
        return MapToResponse(cat, includeChildren: true);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Generate(request.CategoryName)
            : request.Slug;

        if (await _repo.SlugExistsAsync(slug))
            slug = SlugHelper.GenerateUnique(request.CategoryName, DateTime.UtcNow.Ticks.ToString()[^4..]);

        var category = new Category
        {
            CategoryName = request.CategoryName,
            Slug = slug,
            ParentCategoryId = request.ParentCategoryId,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(category);
        return MapToResponse(category);
    }

    public async Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy danh mục.");

        // Không cho set parent = chính nó
        if (request.ParentCategoryId == id)
            throw new InvalidOperationException("Danh mục không thể là cha của chính nó.");

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Generate(request.CategoryName)
            : request.Slug;

        if (await _repo.SlugExistsAsync(slug, id))
            slug = SlugHelper.GenerateUnique(request.CategoryName, DateTime.UtcNow.Ticks.ToString()[^4..]);

        category.CategoryName = request.CategoryName;
        category.Slug = slug;
        category.ParentCategoryId = request.ParentCategoryId;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.IsActive = request.IsActive;

        await _repo.UpdateAsync(category);
        return MapToResponse(category);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy danh mục.");

        if (await _repo.HasChildrenAsync(id))
            throw new InvalidOperationException("Không thể xóa danh mục còn danh mục con.");
        if (await _repo.HasProductsAsync(id))
            throw new InvalidOperationException("Không thể xóa danh mục còn sản phẩm.");

        await _repo.DeleteAsync(category);
    }

    private static CategoryResponse MapToResponse(Category c, bool includeChildren = false) => new()
    {
        CategoryId = c.CategoryId,
        CategoryName = c.CategoryName,
        Slug = c.Slug,
        ParentCategoryId = c.ParentCategoryId,
        ParentCategoryName = c.ParentCategory?.CategoryName,
        Description = c.Description,
        ImageUrl = c.ImageUrl,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt,
        Children = includeChildren
            ? c.InverseParentCategory.Select(ch => MapToResponse(ch)).ToList()
            : new()
    };
}

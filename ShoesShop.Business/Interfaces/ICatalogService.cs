namespace ShoesShop.Business.Interfaces;

// UC-26
public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> GetByIdAsync(int id);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest request);
    Task DeleteAsync(int id);
}

// UC-27
public interface IBrandService
{
    Task<List<BrandResponse>> GetAllAsync();
    Task<BrandResponse> GetByIdAsync(int id);
    Task<BrandResponse> CreateAsync(CreateBrandRequest request);
    Task<BrandResponse> UpdateAsync(int id, UpdateBrandRequest request);
    Task DeleteAsync(int id);
}

// UC-28
public interface ISizeColorService
{
    Task<List<SizeResponse>> GetAllSizesAsync();
    Task<SizeResponse> CreateSizeAsync(CreateSizeRequest request);
    Task<SizeResponse> UpdateSizeAsync(int id, UpdateSizeRequest request);
    Task DeleteSizeAsync(int id);

    Task<List<ColorResponse>> GetAllColorsAsync();
    Task<ColorResponse> CreateColorAsync(CreateColorRequest request);
    Task<ColorResponse> UpdateColorAsync(int id, UpdateColorRequest request);
    Task DeleteColorAsync(int id);
}

// UC-29 (Admin CRUD) + F4 Browse (public)
public interface IProductService
{
    // ── Admin CRUD ──────────────────────────────────────────────────────
    Task<ProductListResponse> GetListAsync(string? search, int? categoryId, int? brandId, bool? isActive, int page, int pageSize);
    Task<ProductDetailResponse> GetByIdAsync(int id);
    Task<ProductDetailResponse> CreateAsync(CreateProductRequest request);
    Task<ProductDetailResponse> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
    Task<ProductImageResponse> AddImageAsync(int productId, AddProductImageRequest request);
    Task DeleteImageAsync(int productId, int imageId);
    Task<ProductImageResponse> SetPrimaryImageAsync(int productId, int imageId);

    // ── F4 Public Browse (UC-01, UC-03, UC-04) ─────────────────────────
    Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync();
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword);
    Task<ProductDto?> GetProductDetailAsync(int id);
}

// UC-30
public interface IProductVariantService
{
    Task<List<VariantResponse>> GetByProductAsync(int productId);
    Task<VariantResponse> CreateAsync(int productId, CreateVariantRequest request);
    Task<VariantResponse> UpdateAsync(int productId, int variantId, UpdateVariantRequest request);
    Task<VariantResponse> UpdateStockAsync(int productId, int variantId, UpdateStockRequest request);
    Task DeleteAsync(int productId, int variantId);
}

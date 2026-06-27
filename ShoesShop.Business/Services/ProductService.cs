using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;
using ShoesShop.Shared.DTOs;

namespace ShoesShop.Business.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo) => _repo = repo;

    // ── F3 Admin CRUD ───────────────────────────────────────────────────

    public async Task<ProductListResponse> GetListAsync(
        string? search, int? categoryId, int? brandId, bool? isActive, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize;

        var (products, total) = await _repo.GetPaginatedAsync(search, categoryId, brandId, isActive, page, pageSize);

        return new ProductListResponse
        {
            Products = products.Select(p => new ProductListItem
            {
                ProductId        = p.ProductId,
                ProductName      = p.ProductName,
                Slug             = p.Slug,
                CategoryName     = p.Category.CategoryName,
                BrandName        = p.Brand.BrandName,
                BasePrice        = p.BasePrice,
                SalePrice        = p.SalePrice,
                IsActive         = p.IsActive,
                IsFeatured       = p.IsFeatured,
                PrimaryImageUrl  = p.ProductImages.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                CreatedAt        = p.CreatedAt
            }).ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    public async Task<ProductDetailResponse> GetByIdAsync(int id)
    {
        var product = await _repo.GetByIdWithDetailsAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");
        return MapToDetail(product);
    }

    public async Task<ProductDetailResponse> CreateAsync(CreateProductRequest request)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Generate(request.ProductName)
            : request.Slug;

        if (await _repo.SlugExistsAsync(slug))
            slug = SlugHelper.GenerateUnique(request.ProductName, DateTime.UtcNow.Ticks.ToString()[^4..]);

        var product = new Product
        {
            ProductName      = request.ProductName,
            Slug             = slug,
            CategoryId       = request.CategoryId,
            BrandId          = request.BrandId,
            Description      = request.Description,
            ShortDescription = request.ShortDescription,
            BasePrice        = request.BasePrice,
            SalePrice        = request.SalePrice,
            Gender           = request.Gender,
            Material         = request.Material,
            IsActive         = request.IsActive,
            IsFeatured       = request.IsFeatured,
            ViewCount        = 0,
            CreatedAt        = DateTime.UtcNow
        };

        if (request.Images.Any())
        {
            if (!request.Images.Any(i => i.IsPrimary))
                request.Images[0].IsPrimary = true;

            product.ProductImages = request.Images.Select((img, idx) => new ProductImage
            {
                ImageUrl     = img.ImageUrl,
                IsPrimary    = img.IsPrimary,
                DisplayOrder = img.DisplayOrder > 0 ? img.DisplayOrder : idx
            }).ToList();
        }

        await _repo.AddAsync(product);

        var detail = await _repo.GetByIdWithDetailsAsync(product.ProductId);
        return MapToDetail(detail!);
    }

    public async Task<ProductDetailResponse> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Generate(request.ProductName)
            : request.Slug;

        if (await _repo.SlugExistsAsync(slug, id))
            slug = SlugHelper.GenerateUnique(request.ProductName, DateTime.UtcNow.Ticks.ToString()[^4..]);

        product.ProductName      = request.ProductName;
        product.Slug             = slug;
        product.CategoryId       = request.CategoryId;
        product.BrandId          = request.BrandId;
        product.Description      = request.Description;
        product.ShortDescription = request.ShortDescription;
        product.BasePrice        = request.BasePrice;
        product.SalePrice        = request.SalePrice;
        product.Gender           = request.Gender;
        product.Material         = request.Material;
        product.IsActive         = request.IsActive;
        product.IsFeatured       = request.IsFeatured;
        product.UpdatedAt        = DateTime.UtcNow;

        await _repo.UpdateAsync(product);

        var detail = await _repo.GetByIdWithDetailsAsync(id);
        return MapToDetail(detail!);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        product.IsActive  = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(product);
    }

    public async Task<ProductImageResponse> AddImageAsync(int productId, AddProductImageRequest request)
    {
        _ = await _repo.GetByIdAsync(productId)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        if (request.IsPrimary)
            await _repo.ClearPrimaryImageAsync(productId);

        var image = new ProductImage
        {
            ProductId    = productId,
            ImageUrl     = request.ImageUrl,
            IsPrimary    = request.IsPrimary,
            DisplayOrder = request.DisplayOrder
        };
        await _repo.AddImageAsync(image);

        return new ProductImageResponse
        {
            ImageId      = image.ImageId,
            ImageUrl     = image.ImageUrl,
            IsPrimary    = image.IsPrimary,
            DisplayOrder = image.DisplayOrder
        };
    }

    public async Task DeleteImageAsync(int productId, int imageId)
    {
        var image = await _repo.GetImageByIdAsync(imageId)
            ?? throw new KeyNotFoundException("Không tìm thấy ảnh.");

        if (image.ProductId != productId)
            throw new UnauthorizedAccessException("Ảnh không thuộc sản phẩm này.");

        await _repo.DeleteImageAsync(image);
    }

    public async Task<ProductImageResponse> SetPrimaryImageAsync(int productId, int imageId)
    {
        var image = await _repo.GetImageByIdAsync(imageId)
            ?? throw new KeyNotFoundException("Không tìm thấy ảnh.");

        if (image.ProductId != productId)
            throw new UnauthorizedAccessException("Ảnh không thuộc sản phẩm này.");

        await _repo.ClearPrimaryImageAsync(productId);
        image.IsPrimary = true;
        await _repo.AddImageAsync(image);

        return new ProductImageResponse
        {
            ImageId      = image.ImageId,
            ImageUrl     = image.ImageUrl,
            IsPrimary    = true,
            DisplayOrder = image.DisplayOrder
        };
    }

    // ── F4 Public Browse ────────────────────────────────────────────────

    public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync()
    {
        var (products, _) = await _repo.GetPaginatedAsync(null, null, null, true, 1, 50);
        return products.Where(p => p.IsFeatured).Select(MapToProductDto);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return Enumerable.Empty<ProductDto>();
        var (products, _) = await _repo.GetPaginatedAsync(keyword, null, null, true, 1, 100);
        return products.Select(MapToProductDto);
    }

    public async Task<ProductDetailDto?> GetProductDetailAsync(int productId)
    {
        var p = await _repo.GetByIdWithDetailsAsync(productId);
        if (p is null) return null;

        var activeVariants = p.ProductVariants.Where(v => v.IsActive).ToList();

        return new ProductDetailDto
        {
            ProductId        = p.ProductId,
            ProductName      = p.ProductName,
            BrandName        = p.Brand?.BrandName,
            CategoryName     = p.Category?.CategoryName,
            BasePrice        = p.BasePrice,
            SalePrice        = p.SalePrice,
            ShortDescription = p.ShortDescription,
            Description      = p.Description,
            Gender           = p.Gender,
            Material         = p.Material,
            Slug             = p.Slug,

            ImageUrls = p.ProductImages
                         .OrderBy(img => img.DisplayOrder)
                         .Select(img => img.ImageUrl)
                         .ToList(),

            Colors = activeVariants
                      .Where(v => v.Color != null)
                      .GroupBy(v => v.ColorId)
                      .Select(g => new ColorDto
                      {
                          ColorId   = g.First().Color!.ColorId,
                          ColorName = g.First().Color!.ColorName,
                          HexCode   = g.First().Color!.HexCode
                      })
                      .ToList(),

            Sizes = activeVariants
                     .Where(v => v.Size != null)
                     .GroupBy(v => v.SizeId)
                     .Select(g => new SizeDto
                     {
                         SizeId   = g.First().Size!.SizeId,
                         SizeName = g.First().Size!.SizeValue
                     })
                     .OrderBy(s => s.SizeName)
                     .ToList(),

            Variants = activeVariants.Select(v => new VariantStockDto
            {
                VariantId     = v.VariantId,
                SizeId        = v.SizeId,
                ColorId       = v.ColorId,
                StockQuantity = v.StockQuantity,
                IsActive      = v.IsActive
            }).ToList()
        };
    }

    // ── Private Mappers ─────────────────────────────────────────────────

    private static ProductDto MapToProductDto(Product p) => new()
    {
        ProductId        = p.ProductId,
        ProductName      = p.ProductName,
        Slug             = p.Slug,
        CategoryId       = p.CategoryId,
        BrandId          = p.BrandId,
        Description      = p.Description,
        ShortDescription = p.ShortDescription,
        BasePrice        = p.BasePrice,
        SalePrice        = p.SalePrice,
        Gender           = p.Gender,
        Material         = p.Material,
        ImageUrls        = p.ProductImages
                            .OrderBy(i => i.DisplayOrder)
                            .Select(i => i.ImageUrl)
                            .ToList(),
        Variants         = p.ProductVariants.Where(v => v.IsActive).Select(v => new ProductVariantDto
        {
            VariantId     = v.VariantId,
            SizeId        = v.SizeId,
            SizeValue     = v.Size.SizeValue,
            ColorId       = v.ColorId,
            ColorName     = v.Color.ColorName,
            HexCode       = v.Color.HexCode,
            SKU           = v.Sku,
            Price         = v.Price,
            StockQuantity = v.StockQuantity
        }).ToList()
    };

    private static ProductDetailResponse MapToDetail(Product p) => new()
    {
        ProductId        = p.ProductId,
        ProductName      = p.ProductName,
        Slug             = p.Slug,
        CategoryId       = p.CategoryId,
        CategoryName     = p.Category.CategoryName,
        BrandId          = p.BrandId,
        BrandName        = p.Brand.BrandName,
        Description      = p.Description,
        ShortDescription = p.ShortDescription,
        BasePrice        = p.BasePrice,
        SalePrice        = p.SalePrice,
        Gender           = p.Gender,
        Material         = p.Material,
        IsActive         = p.IsActive,
        IsFeatured       = p.IsFeatured,
        ViewCount        = p.ViewCount,
        CreatedAt        = p.CreatedAt,
        UpdatedAt        = p.UpdatedAt,
        Images           = p.ProductImages.Select(i => new ProductImageResponse
        {
            ImageId      = i.ImageId,
            ImageUrl     = i.ImageUrl,
            IsPrimary    = i.IsPrimary,
            DisplayOrder = i.DisplayOrder
        }).ToList(),
        Variants         = p.ProductVariants.Select(v => new VariantResponse
        {
            VariantId     = v.VariantId,
            ProductId     = v.ProductId,
            SizeId        = v.SizeId,
            SizeValue     = v.Size.SizeValue,
            ColorId       = v.ColorId,
            ColorName     = v.Color.ColorName,
            HexCode       = v.Color.HexCode,
            Sku           = v.Sku,
            Price         = v.Price,
            StockQuantity = v.StockQuantity,
            IsActive      = v.IsActive
        }).ToList()
    };
}
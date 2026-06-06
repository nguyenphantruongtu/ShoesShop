using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Business.Services;

public class ProductVariantService : IProductVariantService
{
    private readonly IProductVariantRepository _repo;
    private readonly IProductRepository _productRepo;

    public ProductVariantService(IProductVariantRepository repo, IProductRepository productRepo)
    {
        _repo = repo;
        _productRepo = productRepo;
    }

    public async Task<List<VariantResponse>> GetByProductAsync(int productId)
        => (await _repo.GetByProductIdAsync(productId)).Select(MapVariant).ToList();

    public async Task<VariantResponse> CreateAsync(int productId, CreateVariantRequest request)
    {
        // Ensure product exists
        var product = await _productRepo.GetByIdAsync(productId)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        if (await _repo.VariantComboExistsAsync(productId, request.SizeId, request.ColorId))
            throw new InvalidOperationException("Variant với Size+Color này đã tồn tại cho sản phẩm.");

        // Auto-generate SKU if not provided
        var sku = string.IsNullOrWhiteSpace(request.Sku)
            ? $"SKU-{productId}-{request.SizeId}-{request.ColorId}-{DateTime.UtcNow.Ticks % 10000}"
            : request.Sku;

        if (await _repo.SkuExistsAsync(sku))
            throw new InvalidOperationException($"SKU '{sku}' đã tồn tại.");

        var variant = new ProductVariant
        {
            ProductId = productId,
            SizeId = request.SizeId,
            ColorId = request.ColorId,
            Sku = sku,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive
        };

        await _repo.AddAsync(variant);

        var saved = await _repo.GetByIdAsync(variant.VariantId);
        return MapVariant(saved!);
    }

    public async Task<VariantResponse> UpdateAsync(int productId, int variantId, UpdateVariantRequest request)
    {
        var variant = await _repo.GetByIdAndProductIdAsync(variantId, productId)
            ?? throw new KeyNotFoundException("Không tìm thấy variant.");

        if (!string.IsNullOrWhiteSpace(request.Sku) && request.Sku != variant.Sku)
        {
            if (await _repo.SkuExistsAsync(request.Sku, variantId))
                throw new InvalidOperationException($"SKU '{request.Sku}' đã tồn tại.");
            variant.Sku = request.Sku;
        }

        variant.Price = request.Price;
        variant.IsActive = request.IsActive;

        await _repo.UpdateAsync(variant);
        return MapVariant(variant);
    }

    public async Task<VariantResponse> UpdateStockAsync(int productId, int variantId, UpdateStockRequest request)
    {
        var variant = await _repo.GetByIdAndProductIdAsync(variantId, productId)
            ?? throw new KeyNotFoundException("Không tìm thấy variant.");

        variant.StockQuantity = request.StockQuantity;
        await _repo.UpdateAsync(variant);
        return MapVariant(variant);
    }

    public async Task DeleteAsync(int productId, int variantId)
    {
        var variant = await _repo.GetByIdAndProductIdAsync(variantId, productId)
            ?? throw new KeyNotFoundException("Không tìm thấy variant.");
        await _repo.DeleteAsync(variant);
    }

    private static VariantResponse MapVariant(ProductVariant v) => new()
    {
        VariantId = v.VariantId,
        ProductId = v.ProductId,
        SizeId = v.SizeId,
        SizeValue = v.Size.SizeValue,
        ColorId = v.ColorId,
        ColorName = v.Color.ColorName,
        HexCode = v.Color.HexCode,
        Sku = v.Sku,
        Price = v.Price,
        StockQuantity = v.StockQuantity,
        IsActive = v.IsActive
    };
}

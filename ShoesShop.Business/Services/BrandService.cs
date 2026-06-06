using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Business.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _repo;
    public BrandService(IBrandRepository repo) => _repo = repo;

    public async Task<List<BrandResponse>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(Map).ToList();

    public async Task<BrandResponse> GetByIdAsync(int id)
    {
        var brand = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy thương hiệu.");
        return Map(brand);
    }

    public async Task<BrandResponse> CreateAsync(CreateBrandRequest request)
    {
        if (await _repo.NameExistsAsync(request.BrandName))
            throw new InvalidOperationException($"Thương hiệu '{request.BrandName}' đã tồn tại.");

        var brand = new Brand
        {
            BrandName = request.BrandName,
            LogoUrl = request.LogoUrl,
            Description = request.Description,
            IsActive = request.IsActive
        };
        await _repo.AddAsync(brand);
        return Map(brand);
    }

    public async Task<BrandResponse> UpdateAsync(int id, UpdateBrandRequest request)
    {
        var brand = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy thương hiệu.");

        if (await _repo.NameExistsAsync(request.BrandName, id))
            throw new InvalidOperationException($"Tên thương hiệu '{request.BrandName}' đã được dùng.");

        brand.BrandName = request.BrandName;
        brand.LogoUrl = request.LogoUrl;
        brand.Description = request.Description;
        brand.IsActive = request.IsActive;

        await _repo.UpdateAsync(brand);
        return Map(brand);
    }

    public async Task DeleteAsync(int id)
    {
        var brand = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy thương hiệu.");

        if (await _repo.HasProductsAsync(id))
            throw new InvalidOperationException("Không thể xóa thương hiệu còn sản phẩm.");

        await _repo.DeleteAsync(brand);
    }

    private static BrandResponse Map(Brand b) => new()
    {
        BrandId = b.BrandId,
        BrandName = b.BrandName,
        LogoUrl = b.LogoUrl,
        Description = b.Description,
        IsActive = b.IsActive
    };
}

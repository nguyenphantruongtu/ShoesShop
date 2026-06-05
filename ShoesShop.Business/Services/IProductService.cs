using ShoesShop.Shared.DTOs;
using ShoesShop.Data.Entities;

namespace ShoesShop.Business.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync();
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword);
    Task<ProductDto?> GetProductDetailAsync(int id);
}
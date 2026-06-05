using AutoMapper;
using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Repositories;
using ShoesShop.Shared.DTOs;

namespace ShoesShop.Business.Services;

public class ProductService : IProductService
{
    private readonly ProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductService(ProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync()
    {
        var products = await _productRepository.GetFeaturedProductsAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword)
    {
        var products = await _productRepository.SearchProductsAsync(keyword);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetProductDetailAsync(int id)
    {
        var product = await _productRepository.GetProductDetailAsync(id);
        if (product == null) return null;

        return _mapper.Map<ProductDto>(product);
    }
}
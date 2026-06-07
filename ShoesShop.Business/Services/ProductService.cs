using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Interfaces;
using ShoesShop.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoesShop.Business.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository; // Sửa từ ProductRepository sang IProductRepository
    private readonly IMapper _mapper;

    // Inject thông qua Interface để đảm bảo tính lỏng lẻo (Loosely Coupled) của kiến trúc đa tầng
    public ProductService(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    // UC-01: Lấy sản phẩm nổi bật (Dùng AutoMapper map từ Entity sang ProductDto)
    public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync()
    {
        var products = await _productRepository.GetFeaturedProductsAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    // UC-03: Tìm kiếm theo từ khóa (Dùng AutoMapper)
    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword)
    {
        var products = await _productRepository.SearchProductsAsync(keyword);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    // UC-04: Lấy chi tiết sản phẩm kèm slide ảnh, biến thể size, màu (F5)
    public async Task<ProductDetailDto?> GetProductDetailAsync(int productId)
    {
        // 1. Gọi xuống tầng Data qua Interface để lấy dữ liệu từ DB
        var product = await _productRepository.GetProductDetailAsync(productId);

        if (product == null) return null;

        // 2. Map dữ liệu thủ công sang ProductDetailDto để xử lý logic bóc tách Distinct cho Size và Color
        var detailDto = new ProductDetailDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            BasePrice = product.BasePrice,
            SalePrice = product.SalePrice,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Gender = product.Gender,

            // Lấy danh sách link ảnh của sản phẩm
            ImageUrls = product.ProductImages
                                .OrderBy(img => img.DisplayOrder)
                                .Select(img => img.ImageUrl)
                                .ToList(),

            // Gom danh sách Màu sắc không trùng lặp (Distinct) từ các biến thể (Variants)
            Colors = product.ProductVariants
                            .Where(v => v.Color != null)
                            .Select(v => new ColorDto
                            {
                                ColorId = v.Color!.ColorId,
                                ColorName = v.Color.ColorName,
                                HexCode = v.Color.HexCode
                            })
                            .GroupBy(c => c.ColorId).Select(g => g.First()).ToList(),

            // Gom danh sách Kích cỡ không trùng lặp từ các biến thể
            Sizes = product.ProductVariants
                           .Where(v => v.Size != null)
                           .Select(v => new SizeDto
                           {
                               SizeId = v.Size!.SizeId,
                               SizeName = v.Size.SizeValue // Giữ nguyên SizeValue theo Database của bạn
                           })
                           .GroupBy(s => s.SizeId).Select(g => g.First()).OrderBy(s => s.SizeName).ToList()
        };

        return detailDto;
    }
}
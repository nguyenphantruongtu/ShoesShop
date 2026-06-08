using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Repositories.Interfaces;
using ShoesShop.Shared.DTOs;

namespace ShoesShop.API.Controllers;

/// <summary>
/// F4. Product Browse (Guest / Customer)
/// UC-01: GET /api/products/featured   — sản phẩm nổi bật (trang chủ)
/// UC-02: GET /api/products            — danh sách + OData filter/sort/paging
/// UC-03: GET /api/products/search     — tìm kiếm theo keyword
/// UC-04: GET /api/products/{id}       — chi tiết sản phẩm
/// </summary>
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IProductRepository _productRepository;

    public ProductsController(IProductService productService, IProductRepository productRepository)
    {
        _productService    = productService;
        _productRepository = productRepository;
    }

    /// <summary>UC-01: Sản phẩm nổi bật cho trang chủ</summary>
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var result = await _productService.GetFeaturedProductsAsync();
        return Ok(new ApiResponse<IEnumerable<ProductDto>> { Success = true, Data = result });
    }

    /// <summary>UC-02: Danh sách sản phẩm hỗ trợ OData (filter/sort/paging)</summary>
    [HttpGet]
    [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
    public IActionResult Get()
    {
        var dtoQuery = _productRepository
            .GetQueryable()
            .Where(p => p.IsActive)
            .ProjectTo<ProductDto>(
                HttpContext.RequestServices
                    .GetRequiredService<AutoMapper.IConfigurationProvider>()
            );
        return Ok(dtoQuery);
    }

    /// <summary>UC-03: Tìm kiếm sản phẩm theo keyword</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        var result = await _productService.SearchProductsAsync(keyword);
        return Ok(new ApiResponse<IEnumerable<ProductDto>> { Success = true, Data = result });
    }

    /// <summary>UC-04: Chi tiết sản phẩm (slider ảnh, variant size/màu, stock)</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var result = await _productService.GetProductDetailAsync(id);
        if (result == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Không tìm thấy sản phẩm." });
        return Ok(new ApiResponse<ProductDetailDto> { Success = true, Data = result });
    }
}

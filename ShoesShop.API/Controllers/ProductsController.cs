using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Interfaces;
using ShoesShop.Shared.DTOs;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IProductRepository _productRepository;

    public ProductsController(IProductService productService, IProductRepository productRepository)
    {
        _productService = productService;
        _productRepository = productRepository;
    }

    // UC-01: Lấy danh sách sản phẩm nổi bật ở Trang chủ
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var result = await _productService.GetFeaturedProductsAsync();
        return Ok(new ApiResponse<IEnumerable<ProductDto>> { Success = true, Data = result });
    }

    // UC-02: Lấy danh sách toàn bộ sản phẩm hỗ trợ OData (filter/sort/paging qua query string)
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

    // UC-03: Tìm kiếm sản phẩm qua keyword
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        var result = await _productService.SearchProductsAsync(keyword);
        return Ok(new ApiResponse<IEnumerable<ProductDto>> { Success = true, Data = result });
    }

    // UC-04: Lấy thông tin chi tiết một sản phẩm theo ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var result = await _productService.GetProductDetailAsync(id);

        if (result == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Không tìm thấy sản phẩm" });

        return Ok(new ApiResponse<ProductDetailDto> { Success = true, Data = result });
    }
}
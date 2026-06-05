using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Repositories;
using ShoesShop.Shared.DTOs;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductsController(IProductService productService, ProductRepository productRepository)
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

    // UC-02: Lấy danh sách toàn bộ sản phẩm hỗ trợ OData
    [HttpGet]
    [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)] // Kích hoạt bộ lọc OData giải mã $filter, $orderby từ client gửi lên
    public IActionResult Get()
    {
        var entityQuery = _productRepository.GetQueryable().Where(p => p.IsActive);

        // 2. Chuyển đổi cấu trúc sang DTO
        var dtoQuery = entityQuery.ProjectTo<ProductDto>(
        HttpContext.RequestServices.GetRequiredService<AutoMapper.IConfigurationProvider>()
    );

        // 3. Trả về đúng kiểu OkObjectResult
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
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _productService.GetProductDetailAsync(id);
        if (result == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Không tìm thấy sản phẩm." });

        return Ok(new ApiResponse<ProductDto> { Success = true, Data = result });
    }
}
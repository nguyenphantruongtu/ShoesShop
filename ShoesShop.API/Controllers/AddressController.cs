using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using System.Security.Claims;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/users/me/addresses")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());

    /// <summary>UC-12: Lấy danh sách địa chỉ giao hàng</summary>
    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        var result = await _addressService.GetMyAddressesAsync(GetUserId());
        return Ok(ApiResponse<List<AddressResponse>>.Ok(result));
    }

    /// <summary>UC-12: Thêm địa chỉ mới</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAddressRequest request)
    {
        var result = await _addressService.CreateAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetAddresses),
            ApiResponse<AddressResponse>.Ok(result, "Thêm địa chỉ thành công."));
    }

    /// <summary>UC-12: Cập nhật địa chỉ</summary>
    [HttpPut("{addressId:int}")]
    public async Task<IActionResult> Update(int addressId, [FromBody] UpdateAddressRequest request)
    {
        try
        {
            var result = await _addressService.UpdateAsync(GetUserId(), addressId, request);
            return Ok(ApiResponse<AddressResponse>.Ok(result, "Cập nhật địa chỉ thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AddressResponse>.Fail(ex.Message));
        }
    }

    /// <summary>UC-12: Xóa địa chỉ</summary>
    [HttpDelete("{addressId:int}")]
    public async Task<IActionResult> Delete(int addressId)
    {
        try
        {
            await _addressService.DeleteAsync(GetUserId(), addressId);
            return Ok(ApiResponse<string>.Ok(string.Empty, "Xóa địa chỉ thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
    }

    /// <summary>UC-12: Set địa chỉ mặc định</summary>
    [HttpPatch("{addressId:int}/set-default")]
    public async Task<IActionResult> SetDefault(int addressId)
    {
        try
        {
            var result = await _addressService.SetDefaultAsync(GetUserId(), addressId);
            return Ok(ApiResponse<AddressResponse>.Ok(result, "Đã đặt làm địa chỉ mặc định."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AddressResponse>.Fail(ex.Message));
        }
    }
}

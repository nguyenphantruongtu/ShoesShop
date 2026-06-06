using ShoesShop.Business.DTOs.Address;

namespace ShoesShop.Business.Interfaces;

public interface IAddressService
{
    Task<List<AddressResponse>> GetMyAddressesAsync(int userId);
    Task<AddressResponse> CreateAsync(int userId, CreateAddressRequest request);
    Task<AddressResponse> UpdateAsync(int userId, int addressId, UpdateAddressRequest request);
    Task DeleteAsync(int userId, int addressId);
    Task<AddressResponse> SetDefaultAsync(int userId, int addressId);
}

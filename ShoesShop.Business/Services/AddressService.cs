using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Business.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;

    public AddressService(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<List<AddressResponse>> GetMyAddressesAsync(int userId)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(userId);
        return addresses.Select(MapToResponse).ToList();
    }

    public async Task<AddressResponse> CreateAsync(int userId, CreateAddressRequest request)
    {
        // Nếu set default → bỏ default cũ trước
        if (request.IsDefault)
            await _addressRepository.ClearDefaultAsync(userId);

        var address = new Address
        {
            UserId = userId,
            RecipientName = request.RecipientName,
            Phone = request.Phone,
            Province = request.Province,
            District = request.District,
            Ward = request.Ward,
            StreetAddress = request.StreetAddress,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow
        };

        await _addressRepository.AddAsync(address);
        return MapToResponse(address);
    }

    public async Task<AddressResponse> UpdateAsync(int userId, int addressId, UpdateAddressRequest request)
    {
        var address = await _addressRepository.GetByIdAndUserIdAsync(addressId, userId)
            ?? throw new KeyNotFoundException("Không tìm thấy địa chỉ.");

        // Nếu set default → bỏ default cũ trước
        if (request.IsDefault && !address.IsDefault)
            await _addressRepository.ClearDefaultAsync(userId);

        address.RecipientName = request.RecipientName;
        address.Phone = request.Phone;
        address.Province = request.Province;
        address.District = request.District;
        address.Ward = request.Ward;
        address.StreetAddress = request.StreetAddress;
        address.IsDefault = request.IsDefault;

        await _addressRepository.UpdateAsync(address);
        return MapToResponse(address);
    }

    public async Task DeleteAsync(int userId, int addressId)
    {
        var address = await _addressRepository.GetByIdAndUserIdAsync(addressId, userId)
            ?? throw new KeyNotFoundException("Không tìm thấy địa chỉ.");

        await _addressRepository.DeleteAsync(address);
    }

    public async Task<AddressResponse> SetDefaultAsync(int userId, int addressId)
    {
        var address = await _addressRepository.GetByIdAndUserIdAsync(addressId, userId)
            ?? throw new KeyNotFoundException("Không tìm thấy địa chỉ.");

        await _addressRepository.ClearDefaultAsync(userId);
        address.IsDefault = true;
        await _addressRepository.UpdateAsync(address);

        return MapToResponse(address);
    }

    private static AddressResponse MapToResponse(Address a) => new()
    {
        AddressId = a.AddressId,
        RecipientName = a.RecipientName,
        Phone = a.Phone,
        Province = a.Province,
        District = a.District,
        Ward = a.Ward,
        StreetAddress = a.StreetAddress,
        IsDefault = a.IsDefault,
        CreatedAt = a.CreatedAt
    };
}

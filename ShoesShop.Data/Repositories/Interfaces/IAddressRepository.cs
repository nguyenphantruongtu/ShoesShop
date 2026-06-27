using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Repositories.Interfaces;

public interface IAddressRepository
{
    Task<List<Address>> GetByUserIdAsync(int userId);
    Task<Address?> GetByIdAsync(int addressId);
    Task<Address?> GetByIdAndUserIdAsync(int addressId, int userId);
    Task AddAsync(Address address);
    Task UpdateAsync(Address address);
    Task DeleteAsync(Address address);
    Task ClearDefaultAsync(int userId);
}

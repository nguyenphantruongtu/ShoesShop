using ShoesShop.Data.Entities;

namespace ShoesShop.Business.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    DateTime GetExpiry();
}

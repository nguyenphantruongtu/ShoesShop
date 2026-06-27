using ShoesShop.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoesShop.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> IsEmailExistsAsync(string email);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);

        // Admin UC-38
        Task<(List<User> Users, int TotalCount)> GetUsersPaginatedAsync(
            string? search, int page, int pageSize);
    }
}

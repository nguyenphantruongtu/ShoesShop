using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoesShop.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ShoeStoreDbContext _context;
        public UserRepository(ShoeStoreDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<User> Users, int TotalCount)> GetUsersPaginatedAsync(
            string? search, int page, int pageSize)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    (u.Phone != null && u.Phone.Contains(search)));
            }

            var total = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, total);
        }
    }
}

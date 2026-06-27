using ShoesShop.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoesShop.Data.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task<Role?> GetRoleByIdAsync(int roleId);
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Interfaces
{
    public interface IProductRepository
    {
        IQueryable<Product> GetQueryable();

        // UC-01: Lấy sản phẩm nổi bật
        Task<IEnumerable<Product>> GetFeaturedProductsAsync();

        // UC-03: Tìm kiếm theo từ khóa
        Task<IEnumerable<Product>> SearchProductsAsync(string keyword);

        // UC-04: Lấy chi tiết sản phẩm kèm slide ảnh, biến thể size, màu 
        Task<Product?> GetProductDetailAsync(int productId);
    }
}
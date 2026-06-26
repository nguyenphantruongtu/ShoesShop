using AutoMapper;
using ShoesShop.Data.Entities;

namespace ShoesShop.Business;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Cấu hình map sản phẩm
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.BrandName,    opt => opt.MapFrom(src => src.Brand != null ? src.Brand.BrandName : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
            .ForMember(dest => dest.ImageUrls,
                       opt => opt.MapFrom(src => src.ProductImages.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList()))
            .ForMember(dest => dest.Variants,
                       opt => opt.MapFrom(src => src.ProductVariants.Where(v => v.IsActive)));

        // Cấu hình map biến thể chi tiết (Bao gồm lấy dữ liệu từ bảng Size và Color lồng bên trong)
        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(dest => dest.SizeValue, opt => opt.MapFrom(src => src.Size.SizeValue))
            .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.Color.ColorName))
            .ForMember(dest => dest.HexCode, opt => opt.MapFrom(src => src.Color.HexCode));
    }
}
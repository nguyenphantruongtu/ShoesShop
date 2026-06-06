using ShoesShop.Shared.DTOs.Address;
using ShoesShop.Shared.DTOs.User;

namespace ShoesShop.Web.Models.Profile;

/// <summary>View model tổng hợp cho trang Profile — chứa thông tin cá nhân + danh sách địa chỉ.</summary>
public class ProfilePageViewModel
{
    public UserProfileResponse Profile { get; set; } = null!;
    public List<AddressResponse> Addresses { get; set; } = new();

    // Form inline để edit profile (pre-fill)
    public UpdateProfileViewModel UpdateProfile { get; set; } = new();

    // Form để add/edit địa chỉ (pre-fill khi edit)
    public AddressFormViewModel AddressForm { get; set; } = new();
}

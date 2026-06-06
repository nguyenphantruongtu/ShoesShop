using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Shared.DTOs.Address;
using ShoesShop.Shared.DTOs.User;
using ShoesShop.Web.Models.Profile;
using ShoesShop.Web.Services;

namespace ShoesShop.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApiService _api;

    public ProfileController(ApiService api)
    {
        _api = api;
    }

    // ═══════════════════════════════════════════════
    //  GET /Profile  — trang tổng hợp
    // ═══════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var profileResp = await _api.GetAsync<UserProfileResponse>("/api/users/me/profile");
        var addressResp = await _api.GetAsync<List<AddressResponse>>("/api/users/me/addresses");

        if (profileResp?.Data == null)
        {
            TempData["Error"] = "Không thể tải thông tin tài khoản.";
            return RedirectToAction("Index", "Home");
        }

        var profile = profileResp.Data;
        var vm = new ProfilePageViewModel
        {
            Profile = profile,
            Addresses = addressResp?.Data ?? new(),
            UpdateProfile = new UpdateProfileViewModel
            {
                FullName    = profile.FullName,
                Phone       = profile.Phone,
                DateOfBirth = profile.DateOfBirth,
                Gender      = profile.Gender,
                AvatarUrl   = profile.AvatarUrl,
            }
        };

        return View(vm);
    }

    // ═══════════════════════════════════════════════
    //  POST /Profile/UpdateProfile
    // ═══════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel form)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Vui lòng kiểm tra lại thông tin.";
            TempData["ActiveTab"] = "profile";
            return RedirectToAction(nameof(Index));
        }

        var request = new UpdateProfileRequest
        {
            FullName    = form.FullName,
            Phone       = form.Phone,
            DateOfBirth = form.DateOfBirth,
            Gender      = form.Gender,
            AvatarUrl   = form.AvatarUrl,
        };

        var (result, _) = await _api.PutAsync<UserProfileResponse>("/api/users/me/profile", request);

        if (result?.Success == true)
            TempData["Success"] = "Cập nhật thông tin thành công!";
        else
            TempData["Error"] = result?.Message ?? "Cập nhật thất bại.";

        TempData["ActiveTab"] = "profile";
        return RedirectToAction(nameof(Index));
    }

    // ═══════════════════════════════════════════════
    //  POST /Profile/CreateAddress
    // ═══════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAddress(AddressFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Vui lòng kiểm tra lại thông tin địa chỉ.";
            TempData["ActiveTab"] = "addresses";
            return RedirectToAction(nameof(Index));
        }

        var request = new CreateAddressRequest
        {
            RecipientName = form.RecipientName,
            Phone         = form.Phone,
            Province      = form.Province,
            District      = form.District,
            Ward          = form.Ward,
            StreetAddress = form.StreetAddress,
            IsDefault     = form.IsDefault,
        };

        var (result, _) = await _api.PostAsync<AddressResponse>("/api/users/me/addresses", request);

        if (result?.Success == true)
            TempData["Success"] = "Thêm địa chỉ mới thành công!";
        else
            TempData["Error"] = result?.Message ?? "Thêm địa chỉ thất bại.";

        TempData["ActiveTab"] = "addresses";
        return RedirectToAction(nameof(Index));
    }

    // ═══════════════════════════════════════════════
    //  POST /Profile/UpdateAddress/{id}
    // ═══════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAddress(int addressId, AddressFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Vui lòng kiểm tra lại thông tin địa chỉ.";
            TempData["ActiveTab"] = "addresses";
            return RedirectToAction(nameof(Index));
        }

        var request = new UpdateAddressRequest
        {
            RecipientName = form.RecipientName,
            Phone         = form.Phone,
            Province      = form.Province,
            District      = form.District,
            Ward          = form.Ward,
            StreetAddress = form.StreetAddress,
            IsDefault     = form.IsDefault,
        };

        var (result, _) = await _api.PutAsync<AddressResponse>($"/api/users/me/addresses/{addressId}", request);

        if (result?.Success == true)
            TempData["Success"] = "Cập nhật địa chỉ thành công!";
        else
            TempData["Error"] = result?.Message ?? "Cập nhật địa chỉ thất bại.";

        TempData["ActiveTab"] = "addresses";
        return RedirectToAction(nameof(Index));
    }

    // ═══════════════════════════════════════════════
    //  POST /Profile/DeleteAddress/{id}
    // ═══════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int addressId)
    {
        var (result, _) = await _api.DeleteAsync<string>($"/api/users/me/addresses/{addressId}");

        if (result?.Success == true)
            TempData["Success"] = "Đã xóa địa chỉ.";
        else
            TempData["Error"] = result?.Message ?? "Xóa địa chỉ thất bại.";

        TempData["ActiveTab"] = "addresses";
        return RedirectToAction(nameof(Index));
    }

    // ═══════════════════════════════════════════════
    //  POST /Profile/SetDefaultAddress/{id}
    // ═══════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefaultAddress(int addressId)
    {
        var (result, _) = await _api.PatchAsync<AddressResponse>($"/api/users/me/addresses/{addressId}/set-default");

        if (result?.Success == true)
            TempData["Success"] = "Đã đặt làm địa chỉ mặc định.";
        else
            TempData["Error"] = result?.Message ?? "Thao tác thất bại.";

        TempData["ActiveTab"] = "addresses";
        return RedirectToAction(nameof(Index));
    }
}

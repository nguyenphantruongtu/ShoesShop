using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;
using System.Text.Json;

namespace ShoesShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class ProductsController : Controller
{
    private readonly ApiService _api;
    public ProductsController(ApiService api) => _api = api;

    // ── LIST ──
    public async Task<IActionResult> Index(string? search, int? categoryId, int? brandId, bool? isActive, int page = 1)
    {
        ViewData["Breadcrumb"] = "Sản phẩm";
        var qs = $"?page={page}&pageSize=12" +
                 (string.IsNullOrEmpty(search)  ? "" : $"&search={Uri.EscapeDataString(search)}") +
                 (categoryId.HasValue           ? $"&categoryId={categoryId}" : "") +
                 (brandId.HasValue              ? $"&brandId={brandId}"       : "") +
                 (isActive.HasValue             ? $"&isActive={isActive}"     : "");
        var resp       = await _api.GetAsync<JsonElement>($"/api/admin/products{qs}");
        var categories = await _api.GetAsync<JsonElement>("/api/admin/categories");
        var brands     = await _api.GetAsync<JsonElement>("/api/admin/brands");
        ViewBag.Categories = categories?.Data;
        ViewBag.Brands     = brands?.Data;
        ViewBag.Search     = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.BrandId    = brandId;
        ViewBag.IsActive   = isActive;
        ViewBag.Page       = page;
        return View(resp?.Data);
    }

    // ── CREATE FORM ──
    public async Task<IActionResult> Create()
    {
        ViewData["Breadcrumb"] = "Thêm sản phẩm";
        await LoadDropdowns();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string productName, string? slug, int categoryId, int brandId,
        string? description, string? shortDescription, decimal basePrice, decimal? salePrice,
        string? gender, string? material, bool isActive = true, bool isFeatured = false)
    {
        var body = new { productName, slug, categoryId, brandId, description, shortDescription, basePrice, salePrice, gender, material, isActive, isFeatured, images = Array.Empty<object>() };
        var (r, _) = await _api.PostAsync<JsonElement>("/api/admin/products", body);
        if (r?.Success == true)
        {
            TempData["Success"] = "Tạo sản phẩm thành công!";
            var pid = r?.Data.GetProperty("productId").GetInt32() ?? 0;
            return RedirectToAction(nameof(Edit), new { id = pid });
        }
        TempData["Error"] = r?.Message ?? "Thất bại";
        await LoadDropdowns();
        return View();
    }

    // ── EDIT FORM ──
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Breadcrumb"] = "Sửa sản phẩm";
        var resp    = await _api.GetAsync<JsonElement>($"/api/admin/products/{id}");
        var sizes   = await _api.GetAsync<JsonElement>("/api/admin/sizes");
        var colors  = await _api.GetAsync<JsonElement>("/api/admin/colors");
        var variants= await _api.GetAsync<JsonElement>($"/api/admin/products/{id}/variants");
        await LoadDropdowns();
        if (resp?.Data == null) return NotFound();
        ViewBag.Sizes    = sizes?.Data;
        ViewBag.Colors   = colors?.Data;
        ViewBag.Variants = variants?.Data;
        return View(resp.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string productName, string? slug, int categoryId, int brandId,
        string? description, string? shortDescription, decimal basePrice, decimal? salePrice,
        string? gender, string? material, bool isActive, bool isFeatured)
    {
        var body = new { productName, slug, categoryId, brandId, description, shortDescription, basePrice, salePrice, gender, material, isActive, isFeatured, images = Array.Empty<object>() };
        var (r, _) = await _api.PutAsync<JsonElement>($"/api/admin/products/{id}", body);
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Cập nhật thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (r, _) = await _api.DeleteAsync<JsonElement>($"/api/admin/products/{id}");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã xóa sản phẩm." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    // ── IMAGES ──
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddImage(int productId, string imageUrl, bool isPrimary = false, int displayOrder = 0)
    {
        var (r, _) = await _api.PostAsync<JsonElement>($"/api/admin/products/{productId}/images", new { imageUrl, isPrimary, displayOrder });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã thêm ảnh!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int productId, int imageId)
    {
        var (r, _) = await _api.DeleteAsync<JsonElement>($"/api/admin/products/{productId}/images/{imageId}");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã xóa ảnh." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimaryImage(int productId, int imageId)
    {
        var (r, _) = await _api.PatchAsync<JsonElement>($"/api/admin/products/{productId}/images/{imageId}/set-primary");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã đặt ảnh chính." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    // ── VARIANTS ──
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVariant(int productId, int sizeId, int colorId, string? sku, decimal? price, int stockQuantity, bool isActive = true)
    {
        var (r, _) = await _api.PostAsync<JsonElement>($"/api/admin/products/{productId}/variants", new { sizeId, colorId, sku, price, stockQuantity, isActive });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã thêm biến thể!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditVariant(int productId, int variantId, string? sku, decimal? price, bool isActive)
    {
        var (r, _) = await _api.PutAsync<JsonElement>($"/api/admin/products/{productId}/variants/{variantId}", new { sku, price, isActive });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Cập nhật biến thể thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStock(int productId, int variantId, int stockQuantity)
    {
        var (r, _) = await _api.PatchAsync<JsonElement>($"/api/admin/products/{productId}/variants/{variantId}/stock",
            new { stockQuantity });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Cập nhật tồn kho thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVariant(int productId, int variantId)
    {
        var (r, _) = await _api.DeleteAsync<JsonElement>($"/api/admin/products/{productId}/variants/{variantId}");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã xóa biến thể." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    private async Task LoadDropdowns()
    {
        var cats   = await _api.GetAsync<JsonElement>("/api/admin/categories");
        var brands = await _api.GetAsync<JsonElement>("/api/admin/brands");
        ViewBag.Categories = cats?.Data;
        ViewBag.Brands     = brands?.Data;
    }
}

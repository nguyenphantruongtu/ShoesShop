using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Shared.DTOs.Auth;
using ShoesShop.Web.Models.Auth;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ShoesShop.Web.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly string _apiBaseUrl;

    public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7214";
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = _httpClientFactory.CreateClient();
        var request = new LoginRequest { Email = model.Email, Password = model.Password };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync($"{_apiBaseUrl}/api/auth/login", content);
            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(json, options);
                var authData = apiResponse?.Data;

                if (authData == null)
                {
                    ModelState.AddModelError(string.Empty, "Đăng nhập thất bại. Vui lòng thử lại.");
                    return View(model);
                }

                await SignInUserAsync(authData, model.RememberMe);

                HttpContext.Session.SetString("JwtToken", authData.Token);

                TempData["Success"] = $"Chào mừng trở lại, {authData.FullName}!";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectBasedOnRole(authData.Role);
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(json, options);
                ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "Email hoặc mật khẩu không đúng.");
            }
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = _httpClientFactory.CreateClient();
        var request = new RegisterRequest
        {
            Email = model.Email,
            FullName = model.FullName,
            Phone = model.Phone,
            Password = model.Password
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync($"{_apiBaseUrl}/api/auth/register", content);
            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(json, options);
                var authData = apiResponse?.Data;

                if (authData != null)
                {
                    await SignInUserAsync(authData, false);
                    HttpContext.Session.SetString("JwtToken", authData.Token);
                }

                TempData["Success"] = "Đăng ký thành công! Chào mừng bạn đến với ShoesShop.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(json, options);
                ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "Đăng ký thất bại. Email có thể đã được sử dụng.");
            }
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var token = HttpContext.Session.GetString("JwtToken");

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                await client.PostAsync($"{_apiBaseUrl}/api/auth/logout", null);
            }
            catch { }
        }

        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        TempData["Success"] = "Bạn đã đăng xuất thành công.";
        return RedirectToAction("Login");
    }

    private async Task SignInUserAsync(AuthResponse authData, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, authData.UserId.ToString()),
            new Claim(ClaimTypes.Email, authData.Email),
            new Claim(ClaimTypes.Name, authData.FullName),
            new Claim(ClaimTypes.Role, authData.Role),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : authData.ExpiresAt
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
    }

    private IActionResult RedirectBasedOnRole(string role)
    {
        return role switch
        {
            "Admin" => RedirectToAction("Index", "Home"),
            "Staff" => RedirectToAction("Index", "Home"),
            _ => RedirectToAction("Index", "Home")
        };
    }
}

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ShoesShop.Web.Services;

/// <summary>
/// Helper service để gọi ShoesShop API với JWT Bearer token từ session.
/// </summary>
public class ApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _baseUrl;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public ApiService(IHttpClientFactory httpClientFactory,
                      IHttpContextAccessor httpContextAccessor,
                      IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7214";
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _httpClientFactory.CreateClient();
        var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<ApiResponse<T>?> GetAsync<T>(string endpoint)
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{_baseUrl}{endpoint}");
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);
    }

    public async Task<(ApiResponse<T>? result, System.Net.HttpStatusCode statusCode)> PostAsync<T>(string endpoint, object body)
    {
        var client = CreateAuthenticatedClient();
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{_baseUrl}{endpoint}", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);
        return (result, response.StatusCode);
    }

    public async Task<(ApiResponse<T>? result, System.Net.HttpStatusCode statusCode)> PutAsync<T>(string endpoint, object body)
    {
        var client = CreateAuthenticatedClient();
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"{_baseUrl}{endpoint}", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);
        return (result, response.StatusCode);
    }

    public async Task<(ApiResponse<T>? result, System.Net.HttpStatusCode statusCode)> PatchAsync<T>(string endpoint)
    {
        var client = CreateAuthenticatedClient();
        var response = await client.PatchAsync($"{_baseUrl}{endpoint}", null);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);
        return (result, response.StatusCode);
    }

    public async Task<(ApiResponse<T>? result, System.Net.HttpStatusCode statusCode)> DeleteAsync<T>(string endpoint)
    {
        var client = CreateAuthenticatedClient();
        var response = await client.DeleteAsync($"{_baseUrl}{endpoint}");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);
        return (result, response.StatusCode);
    }
}

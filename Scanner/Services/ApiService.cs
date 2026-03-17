using Microsoft.AspNetCore.Authentication;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Scanner.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

            _baseUrl = _configuration["ApiSettings:BaseUrl"] ?? string.Empty;

            if (!string.IsNullOrEmpty(_baseUrl))
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
            }

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        }

        private async Task EnsureAuthorizedAsync()
        {
            if (_httpContextAccessor.HttpContext == null) return;

            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            Debug.WriteLine(accessToken);
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = _configuration["ApiSettings:BearerToken"] ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            await EnsureAuthorizedAsync();
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            await EnsureAuthorizedAsync();
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, stringContent);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        public async Task<T?> PutAsync<T>(string endpoint, object? data)
        {
            await EnsureAuthorizedAsync();
            HttpContent contentObj;
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                contentObj = new StringContent(json, Encoding.UTF8, "application/json");
            }
            else
            {
                contentObj = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.PutAsync(endpoint, contentObj);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
    }
}

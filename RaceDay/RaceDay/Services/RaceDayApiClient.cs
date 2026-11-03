using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Mvc.Services
{
    public class RaceDayApiClient
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public RaceDayApiClient(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
        {
            _factory = factory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient CreateClient() => _factory.CreateClient("RaceDayApi");

        private HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            string? apiCookie = _httpContextAccessor.HttpContext?.Session.GetString("ApiSessionCookie");
            if (!string.IsNullOrWhiteSpace(apiCookie))
            {
                request.Headers.TryAddWithoutValidation("Cookie", apiCookie);
            }
            return request;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null)
        {
            var client = CreateClient();
            using var request = CreateRequest(method, url);
            if (body != null)
            {
                request.Content = JsonContent.Create(body);
            }
            return await client.SendAsync(request);
        }

        public async Task<T?> ReadAsync<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) return default;
            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        public async Task<HttpResponseMessage> UploadFileAsync(string url, IFormFile file)
        {
            var client = CreateClient();
            using var request = CreateRequest(HttpMethod.Post, url);
            using var form = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(fileContent, "file", file.FileName);
            request.Content = form;
            return await client.SendAsync(request);
        }

        public static string? ExtractSessionCookie(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var values))
                return null;

            string? sessionHeader = values.FirstOrDefault(v =>
                v.StartsWith(".AspNetCore.Session=", StringComparison.OrdinalIgnoreCase));

            return sessionHeader?.Split(';', 2)[0];
        }
    }
}
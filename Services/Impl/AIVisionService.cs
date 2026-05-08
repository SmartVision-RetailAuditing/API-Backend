using System.Net.Http.Json;
using System.Text.Json;
using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class AIVisionService : IAIVisionService
    {
        private readonly HttpClient _httpClient;

        public AIVisionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var baseUrl = configuration["AIVisionService:BaseUrl"] ?? throw new InvalidOperationException("AIVisionService:BaseUrl is missing.");
            _httpClient.BaseAddress = new Uri(baseUrl);

            var apiKey = configuration["AIVisionService:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
                _httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        }

        public async Task<AiVisionResultDto> AnalyzeShelfAsync(IFormFile image)
        {
            using var content = new MultipartFormDataContent();
            using var stream = image.OpenReadStream();
            using var streamContent = new StreamContent(stream);
            
            content.Add(streamContent, "file", image.FileName);
            
            var response = await _httpClient.PostAsync("analyze", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"AI Vision service returned {(int)response.StatusCode}: {errorBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<AiVisionResultDto>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? throw new InvalidOperationException("AI Vision response was null.");
        }
    }
}
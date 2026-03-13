using System.Net.Http.Json;
using System.Text.Json;
using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class AIVisionService : IAIVisionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _aiServiceUrl;

        public AIVisionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _aiServiceUrl = configuration["AIVisionService:BaseUrl"]
                ?? throw new InvalidOperationException("AIVisionService:BaseUrl is missing.");

            // Opsiyonel API key — henüz belli değil, ileriye açık
            var apiKey = configuration["AIVisionService:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
                _httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        }

        public async Task<AiVisionResultDto> AnalyzeShelfAsync(string imageUrl)
        {
            var payload = new { image_url = imageUrl };

            var response = await _httpClient.PostAsJsonAsync("/analyze", payload);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"AI Vision service returned {(int)response.StatusCode}: {errorBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<AiVisionResultDto>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result is null)
                throw new InvalidOperationException("AI Vision service returned an empty or invalid response.");

            return result;
        }
    }
}
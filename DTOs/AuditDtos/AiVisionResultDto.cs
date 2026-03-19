using System.Text.Json.Serialization;

namespace ApiBackend.DTOs.AuditDtos
{
    // Python AI servisinden gelen JSON response'un C# modeli

    public class AiVisionResultDto
    {
        [JsonPropertyName("azure_blob_url")]
        public string? PostImageAzureUrl { get; set; }

        [JsonPropertyName("products")]
        public List<AiProductDto> Products { get; set; } = new();

        // Aşağıdakiler JSON'dan gelmez, backend'deki Kural Motoru (ComplianceService) doldurur:
        [JsonIgnore]
        public decimal ComplianceScore { get; set; }

        [JsonIgnore]
        public decimal? ShelfSharePercentage { get; set; }

        [JsonIgnore]
        public List<AiIssueDto> Issues { get; set; } = new();
    }
}

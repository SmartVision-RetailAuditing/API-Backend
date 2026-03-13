using System.Text.Json.Serialization;

namespace ApiBackend.DTOs.AuditDtos
{
    // Python AI servisinden gelen JSON response'un C# modeli

    public class AiVisionResultDto
    {
        [JsonPropertyName("compliance_score")]
        public decimal ComplianceScore { get; set; }

        [JsonPropertyName("shelf_share_percentage")]
        public decimal? ShelfSharePercentage { get; set; } // AI dönerse kabul et, yoksa backend hesaplar

        [JsonPropertyName("products")]
        public List<AiProductDto> Products { get; set; } = new();

        [JsonPropertyName("issues")]
        public List<AiIssueDto> Issues { get; set; } = new();
    }
}

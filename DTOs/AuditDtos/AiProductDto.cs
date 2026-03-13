using System.Text.Json.Serialization;

namespace ApiBackend.DTOs.AuditDtos
{
    public class AiProductDto
    {
        [JsonPropertyName("product_code")]
        public string? ProductCode { get; set; }

        [JsonPropertyName("product_name")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("brand_name")]
        public string BrandName { get; set; } = string.Empty;

        [JsonPropertyName("volume")]
        public string? Volume { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("confidence_score")]
        public double ConfidenceScore { get; set; }

        [JsonPropertyName("is_eye_level")]
        public bool IsEyeLevel { get; set; }

        [JsonPropertyName("shelf_position")]
        public int? ShelfPosition { get; set; }

        [JsonPropertyName("bounding_box")]
        public AiBoundingBoxDto BoundingBox { get; set; } = new();
    }
}

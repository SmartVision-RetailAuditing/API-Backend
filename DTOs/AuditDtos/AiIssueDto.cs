using System.Text.Json.Serialization;

namespace ApiBackend.DTOs.AuditDtos
{
    public class AiIssueDto
    {
        [JsonPropertyName("issue_type")]
        public string IssueType { get; set; } = string.Empty; // "MISSING_PRODUCT" vb.

        [JsonPropertyName("severity")]
        public string Severity { get; set; } = string.Empty;  // "LOW" | "MEDIUM" | "HIGH" | "CRITICAL"

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}

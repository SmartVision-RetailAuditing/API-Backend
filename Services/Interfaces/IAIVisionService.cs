using ApiBackend.DTOs.AuditDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IAIVisionService
    {
        /// <summary>
        /// Blob URL'ini Python AI servisine gönderir, analiz sonucunu döner.
        /// </summary>
        Task<AiVisionResultDto> AnalyzeShelfAsync(string imageUrl);
    }
}

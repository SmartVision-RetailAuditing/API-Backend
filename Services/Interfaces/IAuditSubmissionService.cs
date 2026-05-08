using ApiBackend.DTOs.AuditDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IAuditSubmissionService
    {
        /// <summary>
        /// Fotoğrafı alır, AI'a gönderir, sonucu DB'ye yazar, task'ı kapatır.
        /// </summary>
        Task<AuditResultDto> ProcessAuditAsync(IFormFile image, int taskId, int userId);
    }
}

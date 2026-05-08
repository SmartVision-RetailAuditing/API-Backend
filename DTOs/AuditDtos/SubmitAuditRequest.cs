namespace ApiBackend.DTOs.AuditDtos
{
    public class SubmitAuditRequest
    {
        public IFormFile Image { get; set; } = null!;
        public int TaskId { get; set; }
    }
}

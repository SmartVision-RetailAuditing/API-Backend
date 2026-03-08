using ApiBackend.DTOs;
using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,SUPERVISOR")] // Web panelinden sadece yöneticiler erişebilir
    public class AuditsController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditsController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        // GET: api/audits?page=1&size=10&search=migros&status=WARNING
        [HttpGet]
        public async Task<ActionResult<PagedResult<AuditDto>>> GetAudits(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null)
        {
            var result = await _auditService.GetAllAuditsAsync(page, size, search, status);
            return Ok(result);
        }

        // GET: api/audits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AuditDto>> GetAuditById(int id)
        {
            var audit = await _auditService.GetAuditByIdAsync(id);
            if (audit == null) return NotFound(new { message = "Audit not found." });
            return Ok(audit);
        }

        // POST: api/audits
        [HttpPost]
        public async Task<ActionResult<AuditDto>> CreateAudit([FromBody] CreateAuditDto request)
        {
            var createdAudit = await _auditService.CreateAuditAsync(request);
            return CreatedAtAction(nameof(GetAuditById), new { id = createdAudit.Id }, createdAudit);
        }

        // PUT: api/audits/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAudit(int id, [FromBody] UpdateAuditDto request)
        {
            var result = await _auditService.UpdateAuditAsync(id, request);
            if (!result) return NotFound(new { message = "Audit not found." });
            return NoContent();
        }

        // DELETE: api/audits/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")] // Ekstra Güvenlik: Silme işlemini sadece Admin yapabilsin
        public async Task<IActionResult> DeleteAudit(int id)
        {
            var result = await _auditService.DeleteAuditAsync(id);
            if (!result) return NotFound(new { message = "Audit not found." });
            return NoContent();
        }
    }
}

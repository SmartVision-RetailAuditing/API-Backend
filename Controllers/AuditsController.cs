using ApiBackend.DTOs;
using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,SUPERVISOR,FIELD_WORKER")]
    public class AuditsController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly IAuditSubmissionService _submissionService;
        

        public AuditsController(IAuditService auditService, IAuditSubmissionService submissionService)
        {
            _auditService = auditService;
            _submissionService = submissionService;
        }

        // GET: api/Audits?page=1&size=10&search=migros&status=WARNING&storeId=4
        [HttpGet]
        public async Task<ActionResult<PagedResult<AuditDto>>> GetAudits(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] int? storeId = null)   // ← YENİ
        {
            var result = await _auditService.GetAllAuditsAsync(page, size, search, status, storeId);
            return Ok(result);
        }

        // GET: api/Audits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AuditDto>> GetAuditById(int id)
        {
            var audit = await _auditService.GetAuditByIdAsync(id);
            if (audit == null) return NotFound(new { message = "Audit not found." });
            return Ok(audit);
        }

        // POST: api/Audits
        [HttpPost]
        public async Task<ActionResult<AuditDto>> CreateAudit([FromBody] CreateAuditDto request)
        {
            var created = await _auditService.CreateAuditAsync(request);
            return CreatedAtAction(nameof(GetAuditById), new { id = created.Id }, created);
        }

        // PUT: api/Audits/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAudit(int id, [FromBody] UpdateAuditDto request)
        {
            var result = await _auditService.UpdateAuditAsync(id, request);
            if (!result) return NotFound(new { message = "Audit not found." });
            return NoContent();
        }

        // DELETE: api/Audits/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteAudit(int id)
        {
            var result = await _auditService.DeleteAuditAsync(id);
            if (!result) return NotFound(new { message = "Audit not found." });
            return NoContent();
        }

        // POST: api/Audits/submit
        [HttpPost("submit")]
        [Authorize(Roles = "FIELD_WORKER")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitAuditTask([FromForm] SubmitAuditRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            if (request.Image == null || request.Image.Length == 0)
                return BadRequest(new { message = "Fotoğraf zorunludur." });

            try
            {
                var result = await _submissionService.ProcessAuditAsync(request.Image, request.TaskId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (HttpRequestException ex) { return StatusCode(502, new { message = $"Harici servis hatası: {ex.Message}" }); }
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,SUPERVISOR")]
    public class AuditIssuesController : ControllerBase
    {
        private readonly IAuditIssueService _issueService;
        private readonly IEventPublisher _eventPublisher;

        public AuditIssuesController(IAuditIssueService issueService, IEventPublisher eventPublisher)
        {
            _issueService = issueService;
            _eventPublisher = eventPublisher;
        }

        // POST: api/auditissues
        [HttpPost]
        public async Task<ActionResult<AuditIssueDto>> CreateAuditIssue([FromBody] CreateAuditIssueDto request)
        {
            var createdIssue = await _issueService.AddIssueToAuditAsync(request);
            
            await _eventPublisher.PublishAuditIssueCreatedAsync(request.AuditId, createdIssue.Id);

            // 201 Created döner ve Location header'ında bağlı olduğu Audit'in linkini verir
            return CreatedAtAction(nameof(AuditsController.GetAuditById), "Audits", new { id = request.AuditId }, createdIssue);
            
        }

        // DELETE: api/auditissues/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuditIssue(int id)
        {
            var result = await _issueService.DeleteIssueAsync(id);

            if (!result) return NotFound(new { message = "Audit issue not found." });

            return NoContent();
        }
    }
}

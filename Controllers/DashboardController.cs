using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ApiBackend.DTOs.DashboardDtos;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,SUPERVISOR")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("kpis")]
        public async Task<ActionResult<DashboardKpiDto>> GetKpis()
        {
            var kpis = await _dashboardService.GetKpisAsync();
            return Ok(kpis);
        }

        [HttpGet("recent-issues")]
        public async Task<ActionResult<List<RecentIssueDto>>> GetRecentIssues()
        {
            var issues = await _dashboardService.GetRecentIssuesAsync();
            return Ok(issues);
        }

        [HttpGet("recent-audits")]
        public async Task<ActionResult<List<RecentAuditDto>>> GetRecentAudits()
        {
            var audits = await _dashboardService.GetRecentAuditsAsync();
            return Ok(audits);
        }
    }
}

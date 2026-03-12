using ApiBackend.DTOs.AnalyticsDtos;
using ApiBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,SUPERVISOR")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        // GET: api/analytics?days=30
        [HttpGet]
        public async Task<ActionResult<AnalyticsDto>> GetAnalytics(
            [FromQuery] int days = 30)
        {
            var result = await _analyticsService.GetAnalyticsAsync(days);
            return Ok(result);
        }
    }
}
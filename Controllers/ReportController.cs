using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ApiBackend.Models;
using ApiBackend.Models.Context;

namespace ApiBackend.Controllers
{
    [Authorize(Roles = "Supervisor,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly AnalyticsReportContext _analyticsContext;
        private readonly ComplienceReportContext _complienceContext;

        public ReportController(AnalyticsReportContext analyticsContext,ComplienceReportContext complienceContext)
        {
            _analyticsContext = analyticsContext;
            _complienceContext = complienceContext;
        }
        
        // GET: api/report/regionalanalyticsreport [RBAC: Supervisor]
        [Authorize(Roles = "Supervisor,Admin")]
        [HttpGet]
        [Route("regionalanalyticsreport/{regionId}")]
        public async Task<ActionResult<IEnumerable<AnalyticsReportModel>>> GetRegionalAnalyticsReport(long regionId)
        {
            return await _analyticsContext.AnalyticsReport.ToListAsync();
        }

        // GET: api/report/compliencereport [RBAC: Supervisor]
        [Authorize(Roles = "Supervisor,Admin")]
        [HttpGet]
        [Route("compliencereport/{reportId}")]
        public async Task<ActionResult<IEnumerable<ComplienceReportModel>>> GetComplienceReport(long reportId)
        {
            return await _complienceContext.ComplienceReport.ToListAsync();
        }
    }
}

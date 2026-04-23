using ApiBackend.DTOs;
using ApiBackend.DTOs.StoreDtos;
using ApiBackend.Services.Impl;
using ApiBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoresController : ControllerBase
    {
        private readonly IStoreService _storeService;
        private readonly IEventPublisher _eventPublisher;
        private readonly StorePdfExportService  _pdfService;
        private readonly StoreExcelExportService _excelService;

        public StoresController(IStoreService storeService, IEventPublisher eventPublisher, StorePdfExportService pdfService, StoreExcelExportService excelService)
        {
            _storeService = storeService;
            _eventPublisher = eventPublisher;
            _pdfService = pdfService;
            _excelService = excelService;
        }

        // GET: api/stores?page=1&size=10&search=migros
        [HttpGet]
        public async Task<ActionResult<PagedResult<StoreDto>>> GetStores(
            [FromQuery, Range(1,int.MaxValue)] int page = 1,
            [FromQuery,Range(1,100)] int size = 10,
            [FromQuery] string? search = null)
        {
            var result = await _storeService.GetAllStoresAsync(page, size, search);
            return Ok(result);
        }

        // GET: api/stores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StoreDto>> GetStore([Range(1, int.MaxValue)] int id)
        {
            var store = await _storeService.GetStoreByIdAsync(id);
            if (store == null) return NotFound(new { message = "Store not found." });
            return Ok(store);
        }

        // POST: api/stores — Sadece ADMIN
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<StoreDto>> CreateStore([FromBody] CreateStoreDto request)
        {
            var created = await _storeService.CreateStoreAsync(request);
            await _eventPublisher.PublishStoreCreatedAsync(created.Id);
            return CreatedAtAction(nameof(GetStore), new { id = created.Id }, created);
        }

        // PUT: api/stores/5 — Sadece ADMIN
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateStore(
                [Range(1, int.MaxValue)] int id,
                [FromBody] UpdateStoreDto request)
        {
            var result = await _storeService.UpdateStoreAsync(id, request);
            if (!result) return NotFound(new { message = "Store not found." });
            await _eventPublisher.PublishStoreUpdatedAsync(id);
            return NoContent();
        }

        // DELETE: api/stores/5 — Sadece ADMIN
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteStore([Range(1,int.MaxValue)] int id)
        {
            var result = await _storeService.DeleteStoreAsync(id);
            if (!result) return NotFound(new { message = "Store not found." });
            await _eventPublisher.PublishStoreDeletedAsync(id);
            return NoContent();
        }

        // GET: api/Stores/{id}/export/pdf
        [HttpGet("{id:int}/export/pdf")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<IActionResult> ExportPdf(int id)
        {
            try
            {
                var bytes = await _pdfService.GenerateAsync(id);
                return File(bytes, "application/pdf", $"store-{id}-report.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/Stores/export/excel?search=migros
        [HttpGet("export/excel")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<IActionResult> ExportExcel([FromQuery] string? search = null)
        {
            var bytes = await _excelService.GenerateAsync(search);
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"smartvision-stores-{DateTime.UtcNow:yyyyMMdd}.xlsx"
            );
        }
    }
}

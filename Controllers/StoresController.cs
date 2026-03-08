using ApiBackend.DTOs;
using ApiBackend.DTOs.StoreDtos;
using ApiBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoresController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoresController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        // GET: api/stores?page=1&size=10&search=migros
        [HttpGet]
        public async Task<ActionResult<PagedResult<StoreDto>>> GetStores(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? search = null)
        {
            var result = await _storeService.GetAllStoresAsync(page, size, search);
            return Ok(result);
        }

        // GET: api/stores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StoreDto>> GetStore(int id)
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
            return CreatedAtAction(nameof(GetStore), new { id = created.Id }, created);
        }

        // PUT: api/stores/5 — Sadece ADMIN
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateStore(int id, [FromBody] UpdateStoreDto request)
        {
            var result = await _storeService.UpdateStoreAsync(id, request);
            if (!result) return NotFound(new { message = "Store not found." });
            return NoContent();
        }

        // DELETE: api/stores/5 — Sadece ADMIN
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var result = await _storeService.DeleteStoreAsync(id);
            if (!result) return NotFound(new { message = "Store not found." });
            return NoContent();
        }
    }
}
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreDto>>> GetStores([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            // Pagination parametrelerini servise gönderiyoruz
            var stores = await _storeService.GetAllStoresAsync(page, size);
            return Ok(stores);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StoreDto>> GetStore(int id)
        {
            var store = await _storeService.GetStoreByIdAsync(id);
            if (store == null) return NotFound(new { message = "Store not found." });
            return Ok(store);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")] // Only Admins can add stores!
        public async Task<ActionResult<StoreDto>> CreateStore([FromBody] CreateStoreDto request)
        {
            var createdStore = await _storeService.CreateStoreAsync(request);

            // It returns 201 Created and provides a link to the new resource in the header.
            return CreatedAtAction(nameof(GetStore), new { id = createdStore.Id }, createdStore);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateStore(int id, UpdateStoreDto request)
        {
            var result = await _storeService.UpdateStoreAsync(id, request);
            if (!result) return NotFound(new { message = "Store not found." });
            return NoContent(); // 204 Successful but not returning data.
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var result = await _storeService.DeleteStoreAsync(id);
            // Sadece NotFound() değil, mesaj dönmeli
            if (!result) return NotFound(new { message = "Store not found." });
            return NoContent(); // Delete başarılı ise genelde 204 döner, mesaj dönmeye gerek yoktur.
        }
    }
}
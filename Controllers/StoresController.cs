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
        public async Task<ActionResult<IEnumerable<StoreDto>>> GetStores()
        {
            var stores = await _storeService.GetAllStoresAsync();
            return Ok(stores);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StoreDto>> GetStore(int id)
        {
            var store = await _storeService.GetStoreByIdAsync(id);
            if (store == null) return NotFound();
            return Ok(store);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")] // Only Admins can add stores!
        public async Task<ActionResult<StoreDto>> CreateStore(CreateStoreDto request)
        {
            var createdStore = await _storeService.CreateStoreAsync(request);

            // It returns 201 Created and provides a link to the new resource in the header.
            return CreatedAtAction(nameof(GetStore), new { id = createdStore.Id }, createdStore);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateStore(int id, CreateStoreDto request)
        {
            var result = await _storeService.UpdateStoreAsync(id, request);
            if (!result) return NotFound();
            return NoContent(); // 204 Successful but not returning data.
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var result = await _storeService.DeleteStoreAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
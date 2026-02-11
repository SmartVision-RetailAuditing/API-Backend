using ApiBackend.Data;
using ApiBackend.DTOs.TaskDtos;
using ApiBackend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] //No RBAC for testing purposes
    public class StoreController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StoreController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Store
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreDto>>> GetStores()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();
            int userId = int.Parse(userIdString);

            var stores = await _context.Stores
                .OrderBy(t => t.Region)
                .Select(t => new StoreDto
                {
                    StoreId = t.Id,
                    StoreName = t.Name,
                    ChainName = t.ChainName,
                    Latitude = t.Latitude,
                    Longitude = t.Longitude,
                    Address = t.Address,
                    Region = t.Region,
                    CreatedAt = t.CreatedAt,
                })
                .ToListAsync();

            return Ok(stores);
        }

        // GET: api/Store/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<StoreCompliance>>> GetStoreDetails(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();
            int userId = int.Parse(userIdString);

            if (int.IsNegative(id))
                return BadRequest("Id cannot be a negative value");

            var stores = await _context.StoreCompliances
                .Where(t => t.StoreId == id)
                .FirstOrDefaultAsync();

            return Ok(stores);
        }

        // POST: api/Store [admin only]
        [HttpPost]
        public async Task<ActionResult<Store>> CreateStore(StoreDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newStore = new Store
            {
                Id = request.StoreId,
                Name = request.StoreName,
                ChainName = request.ChainName,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Address = request.Address,
                Region = request.Region,
                CreatedAt = request.CreatedAt
            };

            _context.Stores.Add(newStore);
            await _context.SaveChangesAsync();

            return Created();
        }
    }
}

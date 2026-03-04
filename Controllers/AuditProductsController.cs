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
    public class AuditProductsController : ControllerBase
    {
        private readonly IAuditProductService _productService;

        public AuditProductsController(IAuditProductService productService)
        {
            _productService = productService;
        }

        // POST: api/auditproducts
        [HttpPost]
        public async Task<ActionResult<AuditProductDto>> CreateAuditProduct([FromBody] CreateAuditProductDto request)
        {
            var createdProduct = await _productService.AddProductToAuditAsync(request);

            // 201 Created döner ve Location header'ında bağlı olduğu Audit'in linkini verir
            return CreatedAtAction(nameof(AuditsController.GetAuditById), "Audits", new { id = request.AuditId }, createdProduct);
        }

        // PUT: api/auditproducts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuditProduct(int id, [FromBody] UpdateAuditProductDto request)
        {
            var result = await _productService.UpdateProductAsync(id, request);

            if (!result) return NotFound(new { message = "Audit product not found." });

            return NoContent();
        }

        // DELETE: api/auditproducts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuditProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);

            if (!result) return NotFound(new { message = "Audit product not found." });

            return NoContent();
        }
    }
}

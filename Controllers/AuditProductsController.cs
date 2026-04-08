using ApiBackend.DTOs.AuditDtos;
using ApiBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Class-level: tüm authenticated user'lar controller'a girebilir.
                // Rol kontrolü action seviyesine taşındı.
    public class AuditProductsController : ControllerBase
    {
        private readonly IAuditProductService _productService;
        private readonly IEventPublisher _eventPublisher;

        public AuditProductsController(IAuditProductService productService, IEventPublisher eventPublisher)
        {
            _productService = productService;
            _eventPublisher = eventPublisher;
        }

        // POST: api/auditproducts
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        [HttpPost]
        public async Task<ActionResult<AuditProductDto>> CreateAuditProduct([FromBody] CreateAuditProductDto request)
        {
            var createdProduct = await _productService.AddProductToAuditAsync(request);
            await _eventPublisher.PublishAuditProductCreatedAsync(request.AuditId, createdProduct.Id);

            // 201 Created döner ve Location header'ında bağlı olduğu Audit'in linkini verir
            return CreatedAtAction(nameof(AuditsController.GetAuditById), "Audits", new { id = request.AuditId }, createdProduct);
        }

        // PUT: api/auditproducts/5
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuditProduct(int id, [FromBody] UpdateAuditProductDto request)
        {
            var result = await _productService.UpdateProductAsync(id, request);

            if (!result) return NotFound(new { message = "Audit product not found." });
            await _eventPublisher.PublishAuditProductUpdatedAsync(id);

            return NoContent();
        }

        // DELETE: api/auditproducts/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<IActionResult> DeleteAuditProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);

            if (!result) return NotFound(new { message = "Audit product not found." });
            await _eventPublisher.PublishAuditProductDeletedAsync(id);

            return NoContent();
        }

        // ── Yeni endpoint (FIELD_WORKER) ──────────────────────────────────────────

        /// <summary>
        /// PATCH /api/AuditProducts/{id}
        ///
        /// AI'ın hatalı tanıdığı ürünü field worker'ın düzeltmesi için.
        /// Ownership kontrolü, partial update ve audit metrikleri yeniden
        /// hesaplama servis katmanında gerçekleşir.
        /// </summary>
        [HttpPatch("{id:int}")]
        [Authorize(Roles = "FIELD_WORKER")]
        public async Task<IActionResult> FieldWorkerUpdateProduct(
            int id,
            [FromBody] FieldWorkerUpdateAuditProductDto dto)
        {
            // JWT'den userId'yi al
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "Geçersiz token." });

            var result = await _productService.FieldWorkerUpdateProductAsync(id, userId, dto);

            if (result == null)
                return NotFound(new { message = "Ürün bulunamadı." });

            if (result.IsForbidden)
                return Forbid();

            // SignalR: bağlı web client'larına anlık bildir
            await _eventPublisher.PublishAuditProductUpdatedAsync(id);

            return Ok(new
            {
                productId = result.ProductId,
                isManuallyEdited = result.IsManuallyEdited,
                auditId = result.AuditId,
                shelfSharePercentage = result.ShelfSharePercentage,
                complianceScore = result.ComplianceScore,
                status = result.Status,
                brandDistributionJson = result.BrandDistributionJson
            });
        }

    }
}

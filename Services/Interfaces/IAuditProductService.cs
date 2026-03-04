using ApiBackend.DTOs.AuditDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IAuditProductService
    {
        Task<AuditProductDto> AddProductToAuditAsync(CreateAuditProductDto createDto);
        Task<bool> UpdateProductAsync(int id, UpdateAuditProductDto updateDto);
        Task<bool> DeleteProductAsync(int id);
    }
}

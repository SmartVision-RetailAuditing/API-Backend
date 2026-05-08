using ApiBackend.DTOs;
using ApiBackend.DTOs.StoreDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IStoreService
    {
        Task<PagedResult<StoreDto>> GetAllStoresAsync(
            int pageNumber,
            int pageSize,
            string? search = null
        );

        Task<StoreDto?> GetStoreByIdAsync(int id);
        Task<StoreDto> CreateStoreAsync(CreateStoreDto storeDto);
        Task<bool> UpdateStoreAsync(int id, UpdateStoreDto storeDto);
        Task<bool> DeleteStoreAsync(int id);
    }
}
using ApiBackend.DTOs;
using ApiBackend.DTOs.StoreDtos;
using ApiBackend.Entities;

namespace ApiBackend.Repositories.Interfaces
{
    public interface IStoreRepository
    {
        // IQueryable projection ile direkt StoreDto döner
        Task<PagedResult<StoreDto>> GetStoresAsync(
            int pageNumber,
            int pageSize,
            string? search = null
        );

        Task<Store?> GetStoreByIdAsync(int id);
        Task AddStoreAsync(Store store);
        Task UpdateStoreAsync(Store store);
        Task DeleteStoreAsync(Store store);
    }
}

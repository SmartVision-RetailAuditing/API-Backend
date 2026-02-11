using ApiBackend.DTOs.StoreDtos;

namespace ApiBackend.Services.Interfaces
{
    public interface IStoreService
    {
        Task<IEnumerable<StoreDto>> GetAllStoresAsync();
        Task<StoreDto> GetStoreByIdAsync(int id);
        Task<StoreDto> CreateStoreAsync(CreateStoreDto storeDto);
        Task<bool> UpdateStoreAsync(int id, CreateStoreDto storeDto);
        Task<bool> DeleteStoreAsync(int id);
    }
}

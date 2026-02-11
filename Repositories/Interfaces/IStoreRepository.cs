using ApiBackend.Entities;

namespace ApiBackend.Repositories.Interfaces
{
    public interface IStoreRepository
    {
        Task<IEnumerable<Store>> GetAllStoresAsync();
        Task<Store?> GetStoreByIdAsync(int id);
        Task AddStoreAsync(Store store);
        Task UpdateStoreAsync(Store store);
        Task DeleteStoreAsync(Store store);
    }
}

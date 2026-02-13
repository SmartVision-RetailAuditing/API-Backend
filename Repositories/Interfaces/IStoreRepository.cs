using ApiBackend.Entities;

namespace ApiBackend.Repositories.Interfaces
{
    public interface IStoreRepository
    {
        // Task<IEnumerable<Store>> GetAllStoresAsync();
        Task<Store?> GetStoreByIdAsync(int id);
        Task AddStoreAsync(Store store);
        Task UpdateStoreAsync(Store store);
        Task DeleteStoreAsync(Store store);

        // Pagination parametreleri (pageNumber, pageSize)
        Task<IEnumerable<Store>> GetAllStoresAsync(int pageNumber, int pageSize);

        // Tek seferde hem store hem auditleri getiren metot (Performans için)
        Task<IEnumerable<Store>> GetStoresWithAuditsAsync(int pageNumber, int pageSize);

    }
}

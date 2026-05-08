using ApiBackend.DTOs;
using ApiBackend.DTOs.StoreDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _storeRepository;

        public StoreService(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        public async Task<PagedResult<StoreDto>> GetAllStoresAsync(
            int pageNumber,
            int pageSize,
            string? search = null)
        {
            // Tüm hesaplama ve projeksiyon repository'de yapılıyor
            return await _storeRepository.GetStoresAsync(pageNumber, pageSize, search);
        }

        public async Task<StoreDto?> GetStoreByIdAsync(int id)
        {
            var store = await _storeRepository.GetStoreByIdAsync(id);
            if (store == null) return null;

            // ComplianceScore ve Status burada hesaplanıyor (tek kayıt için kabul edilebilir)
            decimal avgScore = 0;
            string status = "Unknown";

            if (store.Audits != null && store.Audits.Any())
            {
                avgScore = Math.Round(store.Audits.Average(a => a.ComplianceScore), 2);
                status = avgScore >= 80 ? "Compliant"
                       : avgScore >= 60 ? "Warning"
                       : "Non-Compliant";
            }

            return new StoreDto
            {
                Id = store.Id,
                Name = store.Name,
                ChainName = store.ChainName,
                Region = store.Region,
                Address = store.Address,
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                ComplianceScore = avgScore,
                Status = status,
                AuditCount = store.Audits?.Count ?? 0
            };
        }

        public async Task<StoreDto> CreateStoreAsync(CreateStoreDto dto)
        {
            var store = new Store
            {
                Name = dto.Name,
                ChainName = dto.ChainName,
                Region = dto.Region,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CreatedAt = DateTime.UtcNow
            };

            await _storeRepository.AddStoreAsync(store);

            return new StoreDto
            {
                Id = store.Id,
                Name = store.Name,
                ChainName = store.ChainName,
                Region = store.Region,
                Address = store.Address,
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                Status = "Unknown",
                ComplianceScore = 0,
                AuditCount = 0
            };
        }

        public async Task<bool> UpdateStoreAsync(int id, UpdateStoreDto dto)
        {
            var store = await _storeRepository.GetStoreByIdAsync(id);
            if (store == null) return false;

            store.Name = dto.Name;
            store.ChainName = dto.ChainName;
            store.Region = dto.Region;
            store.Address = dto.Address;
            store.Latitude = dto.Latitude;
            store.Longitude = dto.Longitude;

            await _storeRepository.UpdateStoreAsync(store);
            return true;
        }

        public async Task<bool> DeleteStoreAsync(int id)
        {
            var store = await _storeRepository.GetStoreByIdAsync(id);
            if (store == null) return false;

            await _storeRepository.DeleteStoreAsync(store);
            return true;
        }
    }
}
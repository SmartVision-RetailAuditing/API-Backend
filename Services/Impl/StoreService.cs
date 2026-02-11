using ApiBackend.DTOs;
using ApiBackend.DTOs.StoreDtos;
using ApiBackend.Entities;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    /// <summary>
    /// Service layer for store-related business logic and operations
    /// Mağaza ile ilgili iş mantığı ve işlemleri için servis katmanı
    /// </summary>
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IAuditRepository _auditRepository;

        /// <summary>
        /// Constructor with dependency injection for store and audit repositories
        /// Mağaza ve denetim repository'leri için dependency injection ile constructor
        /// </summary>
        public StoreService(IStoreRepository storeRepository, IAuditRepository auditRepository)
        {
            _storeRepository = storeRepository;
            _auditRepository = auditRepository;
        }

        /// <summary>
        /// Retrieves all stores from the system with calculated compliance scores
        /// Hesaplanmış uyumluluk puanları ile sistemdeki tüm mağazaları getirir
        /// </summary>
        /// <returns>List of store DTOs with real compliance data / Gerçek uyumluluk verileri ile mağaza DTO'larının listesi</returns>
        public async Task<IEnumerable<StoreDto>> GetAllStoresAsync()
        {
            // Fetch all stores from repository
            // Repository'den tüm mağazaları çek
            var stores = await _storeRepository.GetAllStoresAsync();

            // We need to calculate score for each store
            // Her mağaza için puanı hesaplamamız lazım
            // Note: For performance, this calculation should be done on SQL side in the future, but for now we'll do it in a loop
            // Not: Performans için ileride bu hesaplama SQL tarafında yapılmalı ama şimdilik döngü içinde yapalım
            var storeDtos = new List<StoreDto>();

            foreach (var s in stores)
            {
                // Get audits for this store (we will add this method to repository)
                // Bu mağazaya ait denetimleri getir (Repository'e bu metodu ekleyeceğiz)
                var audits = await _auditRepository.GetAuditsByStoreIdAsync(s.Id);

                decimal avgScore = 0;
                string status = "Unknown";

                if (audits.Any())
                {
                    // Calculate average compliance score from all audits
                    // Tüm denetimlerden ortalama uyumluluk puanını hesapla
                    avgScore = audits.Average(a => a.ComplianceScore);

                    // Simple status logic
                    // Basit bir durum mantığı
                    if (avgScore >= 80) status = "Compliant"; // Green / Yeşil
                    else if (avgScore >= 60) status = "Warning"; // Yellow / Sarı
                    else status = "Non-Compliant"; // Red / Kırmızı
                }

                storeDtos.Add(new StoreDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ChainName = s.ChainName,
                    Region = s.Region,
                    Address = s.Address,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    ComplianceScore = avgScore, // NOW REAL DATA / ARTIK GERÇEK VERİ
                    Status = status // NOW REAL STATUS / ARTIK GERÇEK DURUM
                });
            }
            return storeDtos;
        }

        /// <summary>
        /// Retrieves a specific store by ID with calculated compliance metrics
        /// Hesaplanmış uyumluluk metrikleri ile belirli bir mağazayı ID ile getirir
        /// </summary>
        /// <param name="id">Store ID / Mağaza ID'si</param>
        /// <returns>Store DTO with compliance data or null if not found / Uyumluluk verileri ile mağaza DTO'su veya bulunamazsa null</returns>
        public async Task<StoreDto> GetStoreByIdAsync(int id)
        {
            // Step 1: Get the store
            // Adım 1: Mağazayı getir
            var store = await _storeRepository.GetStoreByIdAsync(id);
            if (store == null) return null;

            // Step 2: Get audits for this store
            // Adım 2: Mağazaya ait denetimleri getir
            var audits = await _auditRepository.GetAuditsByStoreIdAsync(id);

            // Step 3: Calculate average score and status
            // Adım 3: Ortalama puanı ve durumu hesapla
            decimal avgScore = 0;
            string status = "Unknown"; // Default status if no audits exist / Hiç denetim yoksa varsayılan durum

            if (audits.Any())
            {
                // Calculate average score (from ComplianceScore in Audit table)
                // Ortalama puanı hesapla (Audit tablosundaki ComplianceScore üzerinden)
                avgScore = audits.Average(a => a.ComplianceScore);

                // Round to avoid too many decimal places (Optional but recommended)
                // Virgülden sonra çok basamak olmaması için yuvarlayabiliriz (Opsiyonel ama önerilir)
                avgScore = Math.Round(avgScore, 2);

                // Determine status based on score
                // Durumu belirle
                if (avgScore >= 80)
                    status = "Compliant";       // Green (Compliant) / Yeşil (Uyumlu)
                else if (avgScore >= 60)
                    status = "Warning";         // Yellow (Warning) / Sarı (Uyarı)
                else
                    status = "Non-Compliant";   // Red (Non-compliant) / Kırmızı (Uyumsuz)
            }

            // Step 4: Create and return DTO
            // Adım 4: DTO'yu oluştur ve döndür
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
                Status = status
            };
        }

        /// <summary>
        /// Creates a new store in the system
        /// Sistemde yeni bir mağaza oluşturur
        /// </summary>
        /// <param name="storeDto">Store creation DTO / Mağaza oluşturma DTO'su</param>
        /// <returns>Created store DTO / Oluşturulan mağaza DTO'su</returns>
        public async Task<StoreDto> CreateStoreAsync(CreateStoreDto storeDto)
        {
            // DTO to Entity conversion (Manual Mapping)
            // DTO'dan Entity'ye dönüşüm (Manuel Mapping)
            var store = new Store
            {
                Name = storeDto.Name,
                ChainName = storeDto.ChainName,
                Region = storeDto.Region,
                Address = storeDto.Address,
                Latitude = storeDto.Latitude,
                Longitude = storeDto.Longitude,
                CreatedAt = DateTime.UtcNow
            };

            // Save store to database
            // Mağazayı veritabanına kaydet
            await _storeRepository.AddStoreAsync(store);

            // Return the created store
            // Geriye oluşturulan mağazayı dönüyoruz
            return new StoreDto
            {
                Id = store.Id,
                Name = store.Name,
                ChainName = store.ChainName,
                Region = store.Region,
                Address = store.Address,
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                Status = "New",
                ComplianceScore = 0
            };
        }

        /// <summary>
        /// Updates an existing store's information
        /// Mevcut bir mağazanın bilgilerini günceller
        /// </summary>
        /// <param name="id">Store ID / Mağaza ID'si</param>
        /// <param name="storeDto">Updated store data / Güncellenmiş mağaza verisi</param>
        /// <returns>True if successful, false if store not found / Başarılıysa true, mağaza bulunamazsa false</returns>
        public async Task<bool> UpdateStoreAsync(int id, CreateStoreDto storeDto)
        {
            // Find store by ID
            // Mağazayı ID ile bul
            var store = await _storeRepository.GetStoreByIdAsync(id);
            if (store == null) return false;

            // Update operation
            // Güncelleme işlemi
            store.Name = storeDto.Name;
            store.ChainName = storeDto.ChainName;
            store.Region = storeDto.Region;
            store.Address = storeDto.Address;
            store.Latitude = storeDto.Latitude;
            store.Longitude = storeDto.Longitude;

            // Save changes to database
            // Değişiklikleri veritabanına kaydet
            await _storeRepository.UpdateStoreAsync(store);
            return true;
        }

        /// <summary>
        /// Deletes a store from the system
        /// Sistemden bir mağazayı siler
        /// </summary>
        /// <param name="id">Store ID / Mağaza ID'si</param>
        /// <returns>True if successful, false if store not found / Başarılıysa true, mağaza bulunamazsa false</returns>
        public async Task<bool> DeleteStoreAsync(int id)
        {
            // Find store by ID
            // Mağazayı ID ile bul
            var store = await _storeRepository.GetStoreByIdAsync(id);
            if (store == null) return false;

            // Delete store from database
            // Mağazayı veritabanından sil
            await _storeRepository.DeleteStoreAsync(store);
            return true;
        }
    }
}

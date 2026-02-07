using ApiBackend.Data;
using ApiBackend.Entities;
using BCrypt.Net;
using ApiBackend.Entities;

namespace ApiBackend.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Veritabanı yoksa oluştur
            context.Database.EnsureCreated();

            // ---------------------------------------------------------
            // 1. ADIM: KULLANICILAR (Eğer hiç kullanıcı yoksa ekle)
            // ---------------------------------------------------------
            if (!context.Users.Any())
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");

                var users = new User[]
                {
                    new User
                    {
                        FullName = "Sistem Yöneticisi",
                        Email = "admin@sirket.com",
                        PasswordHash = passwordHash,
                        Role = UserRole.ADMIN,
                        EmployeeId = "ADM-001",
                        IsActive = true
                    },
                    new User
                    {
                        FullName = "Ahmet Yılmaz",
                        Email = "ahmet@sirket.com",
                        PasswordHash = passwordHash,
                        Role = UserRole.FIELD_WORKER,
                        EmployeeId = "FW-2025-0042",
                        Phone = "+905551234567",
                        IsActive = true
                    }
                };
                context.Users.AddRange(users);
                context.SaveChanges(); // Önce kullanıcıları kaydet ki ID'leri oluşsun
            }

            // ---------------------------------------------------------
            // 2. ADIM: MAĞAZALAR (Eğer hiç mağaza yoksa ekle)
            // ---------------------------------------------------------
            if (!context.Stores.Any())
            {
                var stores = new Store[]
                {
                    new Store { Name = "Migros MM Kadıköy", ChainName = "Migros", Address = "Caferağa Mah, Kadıköy", Latitude = 40.985, Longitude = 29.025, Region = "Anadolu Yakası" },
                    new Store { Name = "Şok Market Üsküdar", ChainName = "Şok", Address = "Mimar Sinan, Üsküdar", Latitude = 41.025, Longitude = 29.015, Region = "Anadolu Yakası" },
                    new Store { Name = "CarrefourSA Maltepe", ChainName = "CarrefourSA", Address = "Bağdat Cad, Maltepe", Latitude = 40.950, Longitude = 29.100, Region = "Anadolu Yakası" }
                };
                context.Stores.AddRange(stores);
                context.SaveChanges(); // Mağazaları kaydet
            }

            // ---------------------------------------------------------
            // 3. ADIM: GÖREVLER (Eğer hiç görev yoksa ekle)
            // ---------------------------------------------------------
            if (!context.Tasks.Any())
            {
                // İlişkili verileri bul
                var ahmetUser = context.Users.FirstOrDefault(u => u.Email == "ahmet@sirket.com");
                var migrosStore = context.Stores.FirstOrDefault(s => s.Name == "Migros MM Kadıköy");
                var sokStore = context.Stores.FirstOrDefault(s => s.Name == "Şok Market Üsküdar");

                // Eğer kullanıcı veya mağaza silindiyse hata vermesin diye kontrol
                if (ahmetUser != null && migrosStore != null)
                {
                    var tasks = new List<AuditTask>
                    {
                        // Gelecek Görev (Aktif)
                        new AuditTask
                        {
                            UserId = ahmetUser.Id,
                            StoreId = migrosStore.Id,
                            TaskType = TaskType.SHELF_AUDIT,
                            Priority = TaskPriority.HIGH,
                            Status = AuditTaskStatus.PENDING,
                            DueDate = DateTime.UtcNow.AddDays(1), // Yarın
                            Description = "Süt reyonundaki Pınar ürünlerinin dizilimini kontrol et."
                            // Entity'de CreatedAt varsa buraya ekle: CreatedAt = DateTime.UtcNow
                        },
                        // Tamamlanmış Görev (Geçmiş)
                        new AuditTask
                        {
                            UserId = ahmetUser.Id,
                            StoreId = sokStore != null ? sokStore.Id : migrosStore.Id,
                            TaskType = TaskType.PRICE_CHECK,
                            Priority = TaskPriority.MEDIUM,
                            Status = AuditTaskStatus.COMPLETED,
                            DueDate = DateTime.UtcNow.AddDays(-2),
                            CompletedAt = DateTime.UtcNow.AddDays(-1),
                            Description = "Rakip fiyat analizi tamamlandı."
                        }
                    };

                    context.Tasks.AddRange(tasks);
                    context.SaveChanges();
                }
            }
        }
    }
}
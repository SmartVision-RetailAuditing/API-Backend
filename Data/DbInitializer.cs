using ApiBackend.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Data
{
    public static class DbInitializer
    {
        public static void SeedDevData(AppDbContext context)
        {
            // Same as running 'dotnet ef database update'
            context.Database.Migrate();

            // ---------------------------------------------------------
            // Users 
            // ---------------------------------------------------------
            if (context.Users.Any())
                return;


            var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");

            var users = new User[]
            {
                    new User
                    {
                        FullName = "System Administrator",
                        Email = "admin@company.com",
                        PasswordHash = passwordHash,
                        Role = UserRole.ADMIN,
                        EmployeeId = "ADM-001",
                        IsActive = true
                    },
                    new User
                    {
                        FullName = "Ahmet Yilmaz",
                        Email = "ahmet@company.com",
                        PasswordHash = passwordHash,
                        Role = UserRole.FIELD_WORKER,
                        EmployeeId = "FW-2025-0042",
                        Phone = "+905551234567",
                        IsActive = true
                    }
            };
            context.Users.AddRange(users);
            context.SaveChanges();


            // ---------------------------------------------------------
            // Stores
            // ---------------------------------------------------------

            if (context.Stores.Any())
                return;

            var stores = new Store[]
            {
                    new Store { Name = "Migros MM Kadikoy", ChainName = "Migros", Address = "Caferaga Mah, Kadikoy", Latitude = 40.985, Longitude = 29.025, Region = "Anadolu Yakasi" },
                    new Store { Name = "Sok Market Uskudar", ChainName = "Sok", Address = "Mimar Sinan, Uskudar", Latitude = 41.025, Longitude = 29.015, Region = "Anadolu Yakasi" },
                    new Store { Name = "CarrefourSA Maltepe", ChainName = "CarrefourSA", Address = "Bagdat Cad, Maltepe", Latitude = 40.950, Longitude = 29.100, Region = "Anadolu Yakasi" }
            };
            context.Stores.AddRange(stores);
            context.SaveChanges();


            // ---------------------------------------------------------
            // Tasks
            // ---------------------------------------------------------
            if (context.Tasks.Any())
                return;

            // Get example data
            var ahmetUser = context.Users.FirstOrDefault(u => u.Email == "ahmet@company.com");
            var migrosStore = context.Stores.FirstOrDefault(s => s.Name == "Migros MM Kadikoy");
            var sokStore = context.Stores.FirstOrDefault(s => s.Name == "Sok Market Uskudar");

            if (ahmetUser != null && migrosStore != null)
            {
                var tasks = new List<AuditTask>
                    {
                        new AuditTask
                        {
                            UserId = ahmetUser.Id,
                            StoreId = migrosStore.Id,
                            TaskType = TaskType.SHELF_AUDIT,
                            Priority = TaskPriority.HIGH,
                            Status = AuditTaskStatus.PENDING,
                            DueDate = DateTime.UtcNow.AddDays(1),
                            Description = "Check order of Pinar products on the milk shelves"
                        },
                        new AuditTask
                        {
                            UserId = ahmetUser.Id,
                            StoreId = sokStore != null ? sokStore.Id : migrosStore.Id,
                            TaskType = TaskType.PRICE_CHECK,
                            Priority = TaskPriority.MEDIUM,
                            Status = AuditTaskStatus.COMPLETED,
                            DueDate = DateTime.UtcNow.AddDays(-2),
                            CompletedAt = DateTime.UtcNow.AddDays(-1),
                            Description = "Competition price analysis completed."
                        }
                    };

                context.Tasks.AddRange(tasks);
                context.SaveChanges();
            }

        }
    }
}

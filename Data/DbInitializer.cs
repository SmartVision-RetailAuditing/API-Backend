using ApiBackend.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDevData(AppDbContext context)
        {
            // ---------------------------------------------------------
            // USERS
            // ---------------------------------------------------------
            if (!context.Users.Any())
            {
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
                        FullName = "Selçuk Sayın", // Bölge Sorumlusu
                        Email = "supervisor@company.com",
                        PasswordHash = passwordHash,
                        Role = UserRole.SUPERVISOR,
                        EmployeeId = "SUP-2025-001",
                        Phone = "+905559998877",
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
            }

            // ---------------------------------------------------------
            // STORES
            // ---------------------------------------------------------
            if (!context.Stores.Any())
            {
                var stores = new Store[]
                {
                    new Store { Name = "Migros MM Kadikoy", ChainName = "Migros", Address = "Caferaga Mah, Kadikoy", Latitude = 40.985, Longitude = 29.025, Region = "Anadolu Yakasi" },
                    new Store { Name = "Sok Market Uskudar", ChainName = "Sok", Address = "Mimar Sinan, Uskudar", Latitude = 41.025, Longitude = 29.015, Region = "Anadolu Yakasi" },
                    new Store { Name = "CarrefourSA Maltepe", ChainName = "CarrefourSA", Address = "Bagdat Cad, Maltepe", Latitude = 40.950, Longitude = 29.100, Region = "Anadolu Yakasi" }
                };

                context.Stores.AddRange(stores);
                context.SaveChanges();
            }

            // ---------------------------------------------------------
            // TASKS
            // ---------------------------------------------------------
            if (!context.Tasks.Any())
            {
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

            // ---------------------------------------------------------
            // AUDITS + PRODUCTS + ISSUES
            // ---------------------------------------------------------
            if (!context.Audits.Any())
            {
                var ahmetUser = context.Users.FirstOrDefault(u => u.Email == "ahmet@company.com");
                var completedTask = context.Tasks
                    .FirstOrDefault(t =>
                        t.Status == AuditTaskStatus.COMPLETED &&
                        t.Description == "Competition price analysis completed.");

                if (ahmetUser != null && completedTask != null)
                {
                    var audit = new Audit
                    {
                        TaskId = completedTask.Id,
                        StoreId = completedTask.StoreId,
                        UserId = ahmetUser.Id,
                        ImageUrl = "https://example.com/s3-bucket/audit-img-001.jpg",
                        CaptureDate = DateTime.UtcNow.AddDays(-1),
                        ComplianceScore = 85.5m,
                        Status = AuditStatus.WARNING,
                        ShelfSharePercentage = 35.0m,
                        BrandDistributionJson = "{ \"Pinar\": 10, \"Sek\": 5, \"Sutas\": 8 }"
                    };

                    audit.Products = new List<AuditProduct>
                    {
                        new AuditProduct
                        {
                            ProductName = "Pinar Sut 1L",
                            BrandName = "Pinar",
                            Price = 45.00m,
                            IsManuallyEdited = false,
                            ConfidenceScore = 0.98,
                            BoundingBoxX = 10,
                            BoundingBoxY = 20,
                            BoundingBoxWidth = 100,
                            BoundingBoxHeight = 200
                        },
                        new AuditProduct
                        {
                            ProductName = "Sek Sut 1L",
                            BrandName = "Sek",
                            Price = 42.50m,
                            IsManuallyEdited = true,
                            ConfidenceScore = 0.92,
                            BoundingBoxX = 120,
                            BoundingBoxY = 20,
                            BoundingBoxWidth = 100,
                            BoundingBoxHeight = 200
                        }
                    };

                    audit.Issues = new List<AuditIssue>
                    {
                        new AuditIssue
                        {
                            IssueType = IssueType.WRONG_PRICE,
                            Severity = IssueSeverity.MEDIUM,
                            Description = "Sek Süt fiyatı sistemdeki fiyattan düşük."
                        },
                        new AuditIssue
                        {
                            IssueType = IssueType.MISSING_PRODUCT,
                            Severity = IssueSeverity.CRITICAL,
                            Description = "Pınar Laktozsuz Süt rafta yok (Stok Hatası)."
                        }
                    };

                    context.Audits.Add(audit);
                    context.SaveChanges();
                }
            }
        }
    }
}

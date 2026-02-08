using Microsoft.EntityFrameworkCore;
using ApiBackend.Entities;

namespace ApiBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Veritabanı Tablolarımız
        public DbSet<User> Users { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<AuditTask> Tasks { get; set; }
        public DbSet<Audit> Audits { get; set; }
        public DbSet<AuditProduct> AuditProducts { get; set; } // AI Gözlem Verileri
        public DbSet<AuditIssue> AuditIssues { get; set; }     // Hatalar / İhlaller

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enumların veritabanında sayı (0,1) yerine yazı ("ADMIN", "PENDING") 
            // olarak tutulmasını sağlar. Okunabilirlik için çok önemlidir.

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<AuditTask>()
                .Property(t => t.Status)
                .HasConversion<string>();

            modelBuilder.Entity<AuditTask>()
                .Property(t => t.TaskType)
                .HasConversion<string>();

            modelBuilder.Entity<AuditTask>()
                .Property(t => t.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<Audit>()
                .Property(a => a.Status)
                .HasConversion<string>();

            modelBuilder.Entity<AuditIssue>()
                .Property(i => i.IssueType)
                .HasConversion<string>();

            modelBuilder.Entity<AuditIssue>()
                .Property(i => i.Severity)
                .HasConversion<string>();
        }
    }
}

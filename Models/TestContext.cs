using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Models;

public class TestContext : DbContext
{
    public TestContext(DbContextOptions<TestContext> options)
        : base(options)
    {
    }

    public DbSet<TestItem> TestItems { get; set; } = null!;
}

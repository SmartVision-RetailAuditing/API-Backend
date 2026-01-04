using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Models.Context;

public class TaskContext : DbContext
{
    public TaskContext(DbContextOptions<TaskContext> options)
        : base(options)
    {
    }

    public DbSet<TaskModel> Task { get; set; } = null!;
}

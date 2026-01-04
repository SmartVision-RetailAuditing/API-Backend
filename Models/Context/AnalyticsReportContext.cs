using Microsoft.EntityFrameworkCore;

namespace ApiBackend.Models.Context;

public class AnalyticsReportContext : DbContext
{
    public AnalyticsReportContext(DbContextOptions<AnalyticsReportContext> options)
        : base(options)
    {
    }

    public DbSet<AnalyticsReportModel> AnalyticsReport { get; set; } = null!;
}

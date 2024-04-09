using Microsoft.EntityFrameworkCore;
using Orchestrator.DataModels;

namespace Orchestrator.Services;

public class OrchDbContext : DbContext
{
    public DbSet<SavedInfo> SavedInfos { get; set; }

    public OrchDbContext(DbContextOptions<OrchDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        base.OnConfiguring(builder);
    }
}
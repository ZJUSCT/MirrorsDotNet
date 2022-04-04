using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Manager.Models;

public class MirrorContext : DbContext
{
    public MirrorContext(DbContextOptions<MirrorContext> options) : base(options)
    {
    }

    public DbSet<MirrorItem> Mirrors { get; set; }
    public DbSet<FileIndexConfig> IndexConfigs { get; set; }
    public DbSet<MirrorSyncJob> SyncJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MirrorSyncJob>()
            .HasIndex(b => b.Status)
            .HasFilter("[Status] < 3");
    }
}
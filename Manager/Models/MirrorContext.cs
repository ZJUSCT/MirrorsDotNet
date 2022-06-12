using System.Text.Json;
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
            .HasIndex(b => b.Id);
        modelBuilder.Entity<MirrorSyncJob>()
            .HasIndex(b => b.Status)
            .HasFilter("[Status] = 0");
        modelBuilder.Entity<MirrorSyncJob>()
            .HasIndex(b => b.Status)
            .HasFilter("[Status] < 3");
        modelBuilder.Entity<MirrorItem>().Property(p => p.Name).HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions)default),
            v => JsonSerializer.Deserialize<I18N.StringBase>(v, (JsonSerializerOptions)default)
        );
        modelBuilder.Entity<MirrorItem>().Property(p => p.Description).HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions)default),
            v => JsonSerializer.Deserialize<I18N.StringBase>(v, (JsonSerializerOptions)default)
        );
        modelBuilder.Entity<MirrorItem>().Property(p => p.Container).HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions)default),
            v => JsonSerializer.Deserialize<Container>(v, (JsonSerializerOptions)default)
        );
        modelBuilder.Entity<MirrorSyncJob>().Property(p => p.Container).HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions)default),
            v => JsonSerializer.Deserialize<Container>(v, (JsonSerializerOptions)default)
        );
    }
}
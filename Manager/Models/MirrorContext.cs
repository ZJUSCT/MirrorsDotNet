using System.Text.Json;
using System.Collections.Generic;
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
        modelBuilder.Entity<MirrorItem>().Property(p => p.Container).HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions)default),
            v => JsonSerializer.Deserialize<Container>(v, (JsonSerializerOptions)default)
        );
        modelBuilder.Entity<MirrorSyncJob>().Property(p => p.Container).HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions)default),
            v => JsonSerializer.Deserialize<Container>(v, (JsonSerializerOptions)default)
        );
        // modelBuilder.Entity<MirrorItem>().OwnsOne(
        //     p => p.Container, od =>
        //     {
        //         od.Property(p => p.Volumes)
        //             .HasConversion(
        //                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions)default),
        //                 v => JsonSerializer.Deserialize<List<Volume>>(v, (JsonSerializerOptions)default)
        //             );
        //         od.Property(p => p.Environments)
        //             .HasConversion(
        //                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions)default),
        //                 v => JsonSerializer.Deserialize<List<Environment>>(v, (JsonSerializerOptions)default)
        //             );
        //     }
        // );
        // modelBuilder.Entity<MirrorSyncJob>().OwnsOne(
        //     p => p.Container, od =>
        //     {
        //         od.Property(p => p.Volumes)
        //             .HasConversion(
        //                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions)default),
        //                 v => JsonSerializer.Deserialize<List<Volume>>(v, (JsonSerializerOptions)default)
        //             );
        //         od.Property(p => p.Environments)
        //             .HasConversion(
        //                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions)default),
        //                 v => JsonSerializer.Deserialize<List<Environment>>(v, (JsonSerializerOptions)default)
        //             );
        //     }
        // );
    }
}
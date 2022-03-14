using Microsoft.EntityFrameworkCore;

namespace Manager.Models;

public class MirrorContext : DbContext
{
    public MirrorContext(DbContextOptions<MirrorContext> options) : base(options)
    {
    }

    public DbSet<Mirror.MirrorItem> Mirrors { get; set; }
    public DbSet<Mirror.FileIndexConfig> IndexConfigs { get; set; }
}
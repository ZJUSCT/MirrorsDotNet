using Microsoft.EntityFrameworkCore;

namespace Manager.Models;

/// <summary>
/// In-mem database for mirror configs
/// </summary>
public class MirrorConfigContext : DbContext
{
    public MirrorConfigContext(DbContextOptions<MirrorConfigContext> options) : base(options)
    {
    }

    public DbSet<MirrorPackage> Packages { get; set; }
    public DbSet<MirrorRelease> Releases { get; set; }
}
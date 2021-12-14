using Microsoft.EntityFrameworkCore;

namespace Manager.Models;

public class MirrorConfigContext : DbContext
{
    public MirrorConfigContext(DbContextOptions<MirrorConfigContext> options) : base(options)
    {
    }

    public DbSet<MirrorPackage> Packages { get; set; }

    public DbSet<MirrorRelease> Releases { get; set; }
}
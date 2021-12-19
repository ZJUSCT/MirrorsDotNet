#define OLD_SHIM

using Microsoft.EntityFrameworkCore;

namespace Manager.Models;

/// <summary>
/// Persistent database for mirror status
/// </summary>
public class MirrorStatusContext : DbContext
{
    public MirrorStatusContext(DbContextOptions<MirrorStatusContext> options) : base(options)
    {
    }

    public DbSet<MirrorStatus.PackageInfoDto> Packages { get; set; }
    public DbSet<MirrorStatus.ReleaseInfo> Releases { get; set; }
}
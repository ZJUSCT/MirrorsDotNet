using Microsoft.EntityFrameworkCore;
using Orchestrator.DataModels;

namespace Orchestrator.Services;

public class OrchContext : DbContext
{
    public DbSet<SavedInfo> SavedInfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
    }
}
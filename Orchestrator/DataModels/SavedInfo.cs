using System.ComponentModel.DataAnnotations;

namespace Orchestrator.DataModels;

public class SavedInfo
{
    [Key]
    public required string Id { get; set; }
    public MirrorStatus Status { get; set; }
    public DateTime LastSyncAt { get; set; }
    public UInt64 Size { get; set; }
}
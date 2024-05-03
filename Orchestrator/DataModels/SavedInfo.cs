using System.ComponentModel.DataAnnotations;

namespace Orchestrator.DataModels;

public class SavedInfo
{
    [Key] [MaxLength(42)] public required string Id { get; set; }
    public MirrorStatus Status { get; set; }
    public DateTime LastSyncAt { get; set; }
    public DateTime LastSuccessAt { get; set; }
    public ulong Size { get; set; }
}
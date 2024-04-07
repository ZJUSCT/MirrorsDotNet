using System.ComponentModel.DataAnnotations;

namespace Orchestrator.DataModels;

public class SavedInfo
{
    [Key]
    public string Id { get; set; }
    public MirrorStatus Status { get; set; }
    public DateTime LastSyncAt { get; set; }
}
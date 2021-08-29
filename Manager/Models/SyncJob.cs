using System;
using System.ComponentModel.DataAnnotations;

namespace Manager.Models
{
    public enum JobState
    {
        Created,
        Started,
        Finished,
        Failed
    }
    
    public class SyncJob
    {
        [Key]
        public int Id { get; set; }
        public Mirror Mirror { get; set; }
        public JobState State { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
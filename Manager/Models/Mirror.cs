using System;
using System.ComponentModel.DataAnnotations;

namespace Manager.Models
{
    public enum MirrorState
    {
        Enabled,
        Disabled
    }
    
    public class Mirror
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Interval { get; set; }
        public string Provider { get; set; }
    }
}
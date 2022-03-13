namespace Manager.Models;

public class Mirror
{
    public enum Status
    {
        Succeeded,
        Syncing,
        Failed,
        Paused,
        Cached,
        Unknown
    }
}
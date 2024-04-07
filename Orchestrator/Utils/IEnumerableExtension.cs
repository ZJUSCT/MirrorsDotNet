namespace Orchestrator.Utils;

public static class IEnumerableExtension
{
    public static void ForEach<T>(this System.Collections.Generic.IEnumerable<T> enumerable, System.Action<T> action)
    {
        foreach (var e in enumerable)
        {
            action(e);
        }
    }
}
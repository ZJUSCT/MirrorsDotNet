namespace Orchestrator.Utils;

public static class IEnumerableExtension
{
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var e in enumerable) action(e);
    }
}
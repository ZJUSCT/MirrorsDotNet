namespace Orchestrator.Utils;

public static class FileSizeUtil
{
    private static readonly string[] SizeSuffixes = ["B", "KB", "MB", "GB", "TB"];

    public static string ToString(ulong size)
    {
        double d = size;
        var i = 0;
        while (d >= 1024 && i < SizeSuffixes.Length - 1)
        {
            ++i;
            d /= 1024;
        }

        return $"{d:0.##} {SizeSuffixes[i]}";
    }
}
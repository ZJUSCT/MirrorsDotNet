namespace Orchestrator.Utils;

public static class DateTimeExtension
{
    public static long ToUnixTimeSeconds(this DateTime dt) => new DateTimeOffset(dt).ToUnixTimeSeconds();
}

public static class DateTimeConstants
{
    public static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
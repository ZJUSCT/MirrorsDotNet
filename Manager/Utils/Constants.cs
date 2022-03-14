namespace Manager.Utils;

/// <summary>
/// Global Constants
/// </summary>
public static class Constants
{
    public const string ApiVersion = "v2";
    public const string ContentPath = "Data";
    public const string ConfigPath = "Configs";
    public const string SyncConfigPath = $"{ConfigPath}/SyncConfig";
    public const string IndexConfigPath = $"{ConfigPath}/IndexConfig";
    public const string SqliteConnectionString = "Data Source=Status/mirror-status.db";
    public const string MirrorAllCacheKey = "MirrorStatus";
    public const string MirrorItemCacheKeyPrefix = "MirrorItem_";
}
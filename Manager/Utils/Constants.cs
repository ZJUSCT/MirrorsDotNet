namespace Manager.Utils;

/// <summary>
/// Global Constants
/// </summary>
public static class Constants
{
    public const string ApiVersion = "v2";
    public const string ContentPath = "Data";
    private const string ConfigPath = "Configs";
    public const string SyncConfigPath = $"{ConfigPath}/SyncConfig";
    public const string IndexConfigPath = $"{ConfigPath}/IndexConfig";
    public const string MirrorSqliteConnectionString = "Data Source=Status/mirror-status.db";
    public const string HangFireSqliteConnectionString = "Data Source=Status/hangfire-status.db;";
    public const string HangFireServerName = "HangFireCronScheduler";
    public const string HangFireJobPrefix = "SyncJob_";
    public const string PrometheusInfoMetricName = "manager_info";
    public const string PrometheusStatusMetricName = "mirror_status";
}
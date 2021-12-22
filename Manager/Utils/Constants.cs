﻿namespace Manager.Utils;

/// <summary>
/// Global Constants
/// </summary>
public static class Constants
{
    public const string ContentPath = "Data";
    public const string ConfigPath = "Configs";
    public const string SiteConfigPath = $"{ConfigPath}/site.yml";
    public const string ReleaseConfigPath = $"{ConfigPath}/Releases";
    public const string PackageConfigPath = $"{ConfigPath}/Packages";
    public const string SqliteConnectionString = "Data Source=mirror-status.db";
    public const string MirrorStatusCacheKey = "MirrorStatus";
}
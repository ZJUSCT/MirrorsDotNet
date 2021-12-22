using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Manager.Models;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace Manager.Utils;

public class ConfigLoader
{
    /// <summary>
    /// Load YAML mirror configs 
    /// </summary>
    /// <param name="configContext">Mirror config context</param>
    /// <param name="statusContext">Mirror status context</param>
    /// <param name="mapper">Auto mapper instance</param>
    /// <param name="logger">Logger instance</param>
    public static async Task LoadConfigAsync(MirrorConfigContext configContext, MirrorStatusContext statusContext,
        IMapper mapper, ILogger logger)
    {
        var deserializer = new DeserializerBuilder().Build();

        // Load Release Configs
        configContext.Releases.RemoveRange(configContext.Releases);
        var releaseDirInfo = new DirectoryInfo(Constants.ReleaseConfigPath);
        foreach (var fi in releaseDirInfo.GetFiles("*.yml", SearchOption.AllDirectories))
        {
            var releaseConfig = deserializer.Deserialize<MirrorRelease>(await File.ReadAllTextAsync(fi.FullName));
            releaseConfig.Name = Path.GetFileNameWithoutExtension(fi.Name);
            await configContext.Releases.AddAsync(releaseConfig);

            var releaseItem = await statusContext.Releases.FindAsync(releaseConfig.MappedName);
            if (releaseItem == null)
            {
                var newReleaseItem = mapper.Map<MirrorStatus.ReleaseInfo>(releaseConfig);
                await statusContext.Releases.AddAsync(newReleaseItem);
            }
            else
            {
                // update if existed
                releaseItem.Category = releaseConfig.Category;
            }

            logger.LogInformation("Loaded Release Config {ConfigName}", releaseConfig.Name);
        }

        // Load Package Configs
        configContext.Packages.RemoveRange(configContext.Packages);
        var packageDirInfo = new DirectoryInfo(Constants.PackageConfigPath);
        foreach (var fi in packageDirInfo.GetFiles("*.yml", SearchOption.AllDirectories))
        {
            var packageConfig = deserializer.Deserialize<MirrorPackage>(await File.ReadAllTextAsync(fi.FullName));
            packageConfig.Name = Path.GetFileNameWithoutExtension(fi.Name);
            await configContext.Packages.AddAsync(packageConfig);

            var packageItem = await statusContext.Packages.FindAsync(packageConfig.MappedName);
            if (packageItem == null)
            {
                var newPackageItem = mapper.Map<MirrorStatus.PackageInfoDto>(packageConfig);
                newPackageItem.Status = packageConfig.Type switch
                {
                    MirrorType.ReverseProxy => "R",
                    MirrorType.ProxyCache => "C",
                    MirrorType.Normal => "U", // set default status to 'Unknown'
                    _ => "U" // set default status to 'Unknown'
                };
                await statusContext.Packages.AddAsync(newPackageItem);
            }
            else
            {
                // update if existed
                packageItem.Description = packageConfig.Description;
                packageItem.Url = packageConfig.Url;
                packageItem.HelpUrl = packageConfig.HelpUrl;
                packageItem.Upstream = packageConfig.Upstream;
            }

            logger.LogInformation("Loaded Package Config {ConfigName}", packageConfig.Name);
        }

        await configContext.SaveChangesAsync();
        await statusContext.SaveChangesAsync();
    }
}
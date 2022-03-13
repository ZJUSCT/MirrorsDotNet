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
    /// <param name="mirrorContext">Mirror status context</param>
    /// <param name="mapper">Auto mapper instance</param>
    /// <param name="logger">Logger instance</param>
    public static async Task LoadConfigAsync(MirrorContext mirrorContext, IMapper mapper, ILogger logger)
    {
        var deserializer = new DeserializerBuilder().Build();

        // Load Sync Configs
        var syncDirInfo = new DirectoryInfo(Constants.SyncConfigPath);
        foreach (var fi in syncDirInfo.GetFiles("*.yml", SearchOption.AllDirectories))
        {
            var mirrorConfig = deserializer.Deserialize<Mirror.MirrorConfig>(await File.ReadAllTextAsync(fi.FullName));
            mirrorConfig.Id = Path.GetFileNameWithoutExtension(fi.Name);

            var mirrorItem = await mirrorContext.Mirrors.FindAsync(mirrorConfig.Id);
            if (mirrorItem == null)
            {
                var newMirrorItem = mapper.Map<Mirror.MirrorItem>(mirrorConfig);
                await mirrorContext.Mirrors.AddAsync(newMirrorItem);
            }
            else
            {
                // update if existed
                mirrorItem.UpdateFromConfig(mirrorConfig);
            }

            logger.LogInformation("Loaded Mirror Sync Config {ConfigName}", mirrorConfig.Id);
        }

        // TODO: Load Index Configs
        // // Load Package Configs
        // configContext.Packages.RemoveRange(configContext.Packages);
        // var packageDirInfo = new DirectoryInfo(Constants.PackageConfigPath);
        // foreach (var fi in packageDirInfo.GetFiles("*.yml", SearchOption.AllDirectories))
        // {
        //     var packageConfig = deserializer.Deserialize<MirrorPackage>(await File.ReadAllTextAsync(fi.FullName));
        //     packageConfig.Name = Path.GetFileNameWithoutExtension(fi.Name);
        //     await configContext.Packages.AddAsync(packageConfig);
        //
        //     var packageItem = await mirrorContext.Packages.FindAsync(packageConfig.MappedName);
        //     if (packageItem == null)
        //     {
        //         var newPackageItem = mapper.Map<MirrorStatus.PackageInfoDto>(packageConfig);
        //         newPackageItem.Status = packageConfig.Type switch
        //         {
        //             MirrorType.ReverseProxy => "R",
        //             MirrorType.ProxyCache => "C",
        //             MirrorType.Normal => "U", // set default status to 'Unknown'
        //             _ => "U" // set default status to 'Unknown'
        //         };
        //         await mirrorContext.Packages.AddAsync(newPackageItem);
        //     }
        //     else
        //     {
        //         // update if existed
        //         packageItem.Description = packageConfig.Description;
        //         packageItem.Url = packageConfig.Url;
        //         packageItem.HelpUrl = packageConfig.HelpUrl;
        //         packageItem.Upstream = packageConfig.Upstream;
        //     }
        //
        //     logger.LogInformation("Loaded Package Config {ConfigName}", packageConfig.Name);
        // }

        await mirrorContext.SaveChangesAsync();
    }
}
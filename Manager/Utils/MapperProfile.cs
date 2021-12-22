using AutoMapper;
using Manager.Models;

namespace Manager.Utils;

public class MapperProfile: Profile
{
    public MapperProfile()
    {
        CreateMap<MirrorRelease, MirrorStatus.ReleaseInfo>();
        CreateMap<MirrorPackage, MirrorStatus.PackageInfoDto>();
    }
}
using AutoMapper;
using Manager.Models;

namespace Manager.Utils
{
    public class MapperProfile: Profile
    {
        public MapperProfile()
        {
            CreateMap<MirrorRelease, MirrorZ.ReleaseInfo>();
            CreateMap<MirrorPackage, MirrorZ.PackageInfo>();
        }
    }
}
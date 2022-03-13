using AutoMapper;
using Manager.Models;

namespace Manager.Utils;

public class MapperProfile: Profile
{
    public MapperProfile()
    {
        CreateMap<Mirror.MirrorConfig, Mirror.MirrorItem>();
        CreateMap<Mirror.MirrorItem, Mirror.MirrorItemDto>();
    }
}
using AutoMapper;
using Manager.Models;

namespace Manager.Utils;

public class MapperProfile: Profile
{
    public MapperProfile()
    {
        CreateMap<MirrorConfig, MirrorItem>();
        CreateMap<MirrorItem, MirrorItemDto>();
        CreateMap<MirrorSyncJob, MirrorSyncJobDto>();
    }
}
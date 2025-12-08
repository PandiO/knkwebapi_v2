using System.Linq;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping
{
    public class DistrictMappingProfile : Profile
    {
        public DistrictMappingProfile()
        {
            CreateMap<District, DistrictDto>()
                .ForMember(dest => dest.Id, src => src.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, src => src.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, src => src.MapFrom(src => src.Description))
                .ForMember(dest => dest.CreatedAt, src => src.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.AllowEntry, src => src.MapFrom(src => src.AllowEntry))
                .ForMember(dest => dest.AllowExit, src => src.MapFrom(src => src.AllowExit))
                .ForMember(dest => dest.WgRegionId, src => src.MapFrom(src => src.WgRegionId))
                .ForMember(dest => dest.LocationId, src => src.MapFrom(src => src.LocationId))
                .ForMember(dest => dest.TownId, src => src.MapFrom(src => src.TownId))
                .ForMember(dest => dest.StreetIds, src => src.MapFrom(src => src.Streets.Select(s => s.Id).ToList()));

            CreateMap<DistrictDto, District>()
                .ForMember(dest => dest.Id, src => src.MapFrom(src => src.Id ?? 0))
                .ForMember(dest => dest.Name, src => src.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, src => src.MapFrom(src => src.Description))
                .ForMember(dest => dest.CreatedAt, src => src.MapFrom(src => src.CreatedAt ?? System.DateTime.UtcNow))
                .ForMember(dest => dest.AllowEntry, src => src.MapFrom(src => src.AllowEntry ?? true))
                .ForMember(dest => dest.AllowExit, src => src.MapFrom(src => src.AllowExit ?? true))
                .ForMember(dest => dest.WgRegionId, src => src.MapFrom(src => src.WgRegionId))
                .ForMember(dest => dest.LocationId, src => src.MapFrom(src => src.LocationId))
                .ForMember(dest => dest.Location, src => src.Ignore())
                .ForMember(dest => dest.TownId, src => src.MapFrom(src => src.TownId))
                .ForMember(dest => dest.Town, src => src.Ignore())
                .ForMember(dest => dest.Streets, src => src.Ignore())
                .ForMember(dest => dest.Structures, src => src.Ignore());

            CreateMap<District, DistrictListDto>()
                .ForMember(dest => dest.id, src => src.MapFrom(src => src.Id))
                .ForMember(dest => dest.name, src => src.MapFrom(src => src.Name))
                .ForMember(dest => dest.description, src => src.MapFrom(src => src.Description))
                .ForMember(dest => dest.wgRegionId, src => src.MapFrom(src => src.WgRegionId))
                .ForMember(dest => dest.townId, src => src.MapFrom(src => src.TownId))
                .ForMember(dest => dest.townName, src => src.MapFrom(src => src.Town != null ? src.Town.Name : null));
        }
    }
}

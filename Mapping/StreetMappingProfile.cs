using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping
{
    public class StreetMappingProfile : Profile
    {
        public StreetMappingProfile()
        {
            CreateMap<Street, StreetDto>()
                .ForMember(dest => dest.Id, src => src.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, src => src.MapFrom(src => src.Name));

            CreateMap<StreetDto, Street>()
                .ForMember(dest => dest.Id, src => src.MapFrom(src => src.Id ?? 0))
                .ForMember(dest => dest.Name, src => src.MapFrom(src => src.Name))
                .ForMember(dest => dest.Districts, src => src.Ignore())
                .ForMember(dest => dest.Structures, src => src.Ignore());

            CreateMap<Street, StreetListDto>()
                .ForMember(dest => dest.id, src => src.MapFrom(src => src.Id))
                .ForMember(dest => dest.name, src => src.MapFrom(src => src.Name));
        }
    }

    // Extension method for dynamic shaping (optional, can be used in services)
    public static class StreetMappingExtensions
    {
        // Note: Actual shaping will happen in StreetService similar to DistrictService
    }
}

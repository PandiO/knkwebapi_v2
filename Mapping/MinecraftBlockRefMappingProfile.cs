using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping
{
    public class MinecraftBlockRefMappingProfile : Profile
    {
        public MinecraftBlockRefMappingProfile()
        {
            CreateMap<MinecraftBlockRef, MinecraftBlockRefDto>()
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconUrl));
            CreateMap<MinecraftBlockRefCreateDto, MinecraftBlockRef>()
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconUrl));
            CreateMap<MinecraftBlockRefUpdateDto, MinecraftBlockRef>()
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconUrl));
            CreateMap<MinecraftBlockRef, MinecraftBlockRefListDto>()
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconUrl));

            CreateMap<PagedResult<MinecraftBlockRef>, PagedResultDto<MinecraftBlockRefListDto>>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.PageNumber))
                .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
                .ForMember(dest => dest.TotalCount, opt => opt.MapFrom(src => src.TotalCount));
        }
    }
}

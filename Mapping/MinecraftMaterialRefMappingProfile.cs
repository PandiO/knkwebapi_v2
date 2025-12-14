using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping
{
    public class MinecraftMaterialRefMappingProfile : Profile
    {
        public MinecraftMaterialRefMappingProfile()
        {
            CreateMap<MinecraftMaterialRef, MinecraftMaterialRefDto>()
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconUrl));
            CreateMap<MinecraftMaterialRefCreateDto, MinecraftMaterialRef>()
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconUrl));
            CreateMap<MinecraftMaterialRefUpdateDto, MinecraftMaterialRef>()
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconUrl));
            CreateMap<MinecraftMaterialRef, MinecraftMaterialRefListDto>()
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconUrl));

            CreateMap<PagedResult<MinecraftMaterialRef>, PagedResultDto<MinecraftMaterialRefListDto>>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.PageNumber))
                .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
                .ForMember(dest => dest.TotalCount, opt => opt.MapFrom(src => src.TotalCount));
        }
    }
}

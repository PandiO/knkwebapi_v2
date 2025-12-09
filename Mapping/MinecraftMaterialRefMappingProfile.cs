using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping
{
    public class MinecraftMaterialRefMappingProfile : Profile
    {
        public MinecraftMaterialRefMappingProfile()
        {
            CreateMap<MinecraftMaterialRef, MinecraftMaterialRefDto>();
            CreateMap<MinecraftMaterialRefCreateDto, MinecraftMaterialRef>();
            CreateMap<MinecraftMaterialRefUpdateDto, MinecraftMaterialRef>();
            CreateMap<MinecraftMaterialRef, MinecraftMaterialRefListDto>();

            CreateMap<PagedResult<MinecraftMaterialRef>, PagedResultDto<MinecraftMaterialRefListDto>>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.PageNumber))
                .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
                .ForMember(dest => dest.TotalCount, opt => opt.MapFrom(src => src.TotalCount));
        }
    }
}

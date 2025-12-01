namespace knkwebapi_v2.Mapping
{
    using AutoMapper;
    using knkwebapi_v2.Dtos;
    using knkwebapi_v2.Models;

    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<Category, CategoryDto>().ReverseMap()
                .ForMember(dest => dest.Id, src => src.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, src => src.MapFrom(src => src.Name))
                .ForMember(dest => dest.ItemtypeId, src => src.MapFrom(src => src.ItemtypeId))
                .ForMember(dest => dest.ParentCategoryId, src => src.MapFrom(src => src.ParentCategoryId));

            CreateMap<CategoryDto, Category>()
                .ForMember(dest => dest.Id, src => src.MapFrom(src => src.Id ?? 0))
                .ForMember(dest => dest.Name, src => src.MapFrom(src => src.Name))
                .ForMember(dest => dest.ItemtypeId, src => src.MapFrom(src => src.ItemtypeId))
                .ForMember(dest => dest.ParentCategoryId, src => src.MapFrom(src => src.ParentCategoryId));
        }
    }
}
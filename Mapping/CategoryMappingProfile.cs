namespace knkwebapi_v2.Mapping
{
    using AutoMapper;
    using knkwebapi_v2.Dtos;
    using knkwebapi_v2.Models;

    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.Id, src => src.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, src => src.MapFrom(src => src.Name))
                .ForMember(dest => dest.IconMaterialRefId, src => src.MapFrom(src => src.IconMaterialRefId))
                .ForMember(dest => dest.ParentCategoryId, src => src.MapFrom(src => src.ParentCategoryId.HasValue ? src.ParentCategoryId : src.ParentCategory != null ? src.ParentCategory.Id : null))
                .ForMember(dest => dest.ParentCategory, src => src.MapFrom(src => src.ParentCategory))
                // Embedded lightweight icon material ref
                .ForMember(dest => dest.IconMaterialRef, src => src.MapFrom(s => s.IconMaterialRef == null ? null : new MinecraftMaterialRefDto
                {
                    Id = s.IconMaterialRef.Id,
                    NamespaceKey = s.IconMaterialRef.NamespaceKey,
                    LegacyName = s.IconMaterialRef.LegacyName,
                    Category = s.IconMaterialRef.Category,
                    IconUrl = s.IconMaterialRef.IconUrl
                }));

            CreateMap<CategoryDto, Category>()
                .ForMember(dest => dest.Id, src => src.MapFrom(src => src.Id ?? 0))
                .ForMember(dest => dest.Name, src => src.MapFrom(src => src.Name))
                .ForMember(dest => dest.IconMaterialRefId, src => src.MapFrom(src => src.IconMaterialRefId))
                .ForMember(dest => dest.ParentCategoryId, src => src.MapFrom(src => src.ParentCategoryId))
                .ForMember(dest => dest.ParentCategory, src => src.Ignore());
                // .ForMember(dest => dest.ParentCategory, src => src.MapFrom(src => src.ParentCategory));

            CreateMap<Category, CategoryListDto>()
                .ForMember(dest => dest.id, src => src.MapFrom(src => src.Id))
                .ForMember(dest => dest.name, src => src.MapFrom(src => src.Name))
                .ForMember(dest => dest.parentCategoryId, src => src.MapFrom(src => src.ParentCategoryId))
                .ForMember(dest => dest.parentCategoryName, src => src.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ForMember(dest => dest.iconMaterialRefId, src => src.MapFrom(src => src.IconMaterialRefId))
                .ForMember(dest => dest.iconNamespaceKey, src => src.MapFrom(src => src.IconMaterialRef != null ? src.IconMaterialRef.NamespaceKey : null));

        }
    }
}
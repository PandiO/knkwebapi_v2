using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping
{
    public class EntityTypeConfigurationProfile : Profile
    {
        public EntityTypeConfigurationProfile()
        {
            // Model -> ReadDto
            CreateMap<EntityTypeConfiguration, EntityTypeConfigurationReadDto>()
                .ReverseMap();

            // CreateDto -> Model
            CreateMap<EntityTypeConfigurationCreateDto, EntityTypeConfiguration>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // UpdateDto -> Model
            CreateMap<EntityTypeConfigurationUpdateDto, EntityTypeConfiguration>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}

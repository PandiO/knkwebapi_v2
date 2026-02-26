using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping
{
    public class EnchantmentDefinitionMappingProfile : Profile
    {
        public EnchantmentDefinitionMappingProfile()
        {
            // EnchantmentDefinition → Read DTO
            CreateMap<EnchantmentDefinition, EnchantmentDefinitionReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsCustom, opt => opt.MapFrom(src => src.IsCustom))
                .ForMember(dest => dest.MinecraftEnchantmentRefId, opt => opt.MapFrom(src => src.MinecraftEnchantmentRefId))
                .ForMember(dest => dest.BaseEnchantmentRef, opt => opt.MapFrom(src => src.BaseEnchantmentRef))
                .ForMember(dest => dest.DefaultForBlueprints, opt => opt.MapFrom(src =>
                    src.DefaultForBlueprints
                        .Where(df => df.ItemBlueprint != null)
                        .Select(df => df.ItemBlueprint)))
                .ForMember(dest => dest.AbilityDefinition, opt => opt.MapFrom(src => src.AbilityDefinition));

            // Create/Update DTO → EnchantmentDefinition
            CreateMap<EnchantmentDefinitionCreateDto, EnchantmentDefinition>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BaseEnchantmentRef, opt => opt.Ignore())
                .ForMember(dest => dest.AbilityDefinition, opt => opt.Ignore())
                .ForMember(dest => dest.DefaultForBlueprints, opt => opt.Ignore());

            CreateMap<EnchantmentDefinitionUpdateDto, EnchantmentDefinition>()
                .ForMember(dest => dest.BaseEnchantmentRef, opt => opt.Ignore())
                .ForMember(dest => dest.AbilityDefinition, opt => opt.Ignore())
                .ForMember(dest => dest.DefaultForBlueprints, opt => opt.Ignore());

            // EnchantmentDefinition → List DTO
            CreateMap<EnchantmentDefinition, EnchantmentDefinitionListDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.IsCustom, opt => opt.MapFrom(src => src.IsCustom))
                .ForMember(dest => dest.MaxLevel, opt => opt.MapFrom(src => src.BaseEnchantmentRef != null ? src.BaseEnchantmentRef.MaxLevel : 1))
                .ForMember(dest => dest.MinecraftEnchantmentRefId, opt => opt.MapFrom(src => src.MinecraftEnchantmentRefId))
                .ForMember(dest => dest.BaseEnchantmentNamespaceKey, opt => opt.MapFrom(src => src.BaseEnchantmentRef != null ? src.BaseEnchantmentRef.NamespaceKey : null))
                .ForMember(dest => dest.BlueprintCount, opt => opt.MapFrom(src => src.DefaultForBlueprints != null ? src.DefaultForBlueprints.Count : 0));

            // EnchantmentDefinition → Nav DTO
            CreateMap<EnchantmentDefinition, EnchantmentDefinitionNavDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.IsCustom, opt => opt.MapFrom(src => src.IsCustom));

            // MinecraftEnchantmentRef → Nav DTO
            CreateMap<MinecraftEnchantmentRef, MinecraftEnchantmentRefNavDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.NamespaceKey, opt => opt.MapFrom(src => src.NamespaceKey))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.MaxLevel, opt => opt.MapFrom(src => src.MaxLevel))
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconUrl));

            // AbilityDefinition extension mappings
            CreateMap<AbilityDefinition, AbilityDefinitionReadDto>();

            CreateMap<AbilityDefinitionUpsertDto, AbilityDefinition>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EnchantmentDefinitionId, opt => opt.Ignore())
                .ForMember(dest => dest.EnchantmentDefinition, opt => opt.Ignore());

            // Join entity mapping for reading
            CreateMap<ItemBlueprintDefaultEnchantment, ItemBlueprintDefaultEnchantmentDto>()
                .ForMember(dest => dest.ItemBlueprintId, opt => opt.MapFrom(src => src.ItemBlueprintId))
                .ForMember(dest => dest.EnchantmentDefinitionId, opt => opt.MapFrom(src => src.EnchantmentDefinitionId))
                .ForMember(dest => dest.EnchantmentDefinition, opt => opt.MapFrom(src => src.EnchantmentDefinition))
                .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level));
        }
    }
}

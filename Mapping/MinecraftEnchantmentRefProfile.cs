using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping;

public class MinecraftEnchantmentRefProfile : Profile
{
    public MinecraftEnchantmentRefProfile()
    {
        // Read DTOs
        CreateMap<MinecraftEnchantmentRef, MinecraftEnchantmentRefDto>();
        CreateMap<MinecraftEnchantmentRef, MinecraftEnchantmentRefListDto>();

        // Create DTOs
        CreateMap<MinecraftEnchantmentRefCreateDto, MinecraftEnchantmentRef>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsCustom, opt => opt.Ignore()); // Set by service

        // Update DTOs
        CreateMap<MinecraftEnchantmentRefUpdateDto, MinecraftEnchantmentRef>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.NamespaceKey, opt => opt.Ignore())
            .ForMember(dest => dest.IsCustom, opt => opt.Ignore());
    }
}

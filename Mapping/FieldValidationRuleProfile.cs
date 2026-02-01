using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping
{
    /// <summary>
    /// AutoMapper profile for FieldValidationRule entity and DTOs.
    /// </summary>
    public class FieldValidationRuleProfile : Profile
    {
        public FieldValidationRuleProfile()
        {
            // Entity to DTO
            CreateMap<FieldValidationRule, FieldValidationRuleDto>()
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("o")));
            
            // Create DTO to Entity
            CreateMap<CreateFieldValidationRuleDto, FieldValidationRule>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.FormField, o => o.Ignore())
                .ForMember(d => d.DependsOnField, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore());
            
            // Update DTO to Entity
            CreateMap<UpdateFieldValidationRuleDto, FieldValidationRule>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.FormFieldId, o => o.Ignore())
                .ForMember(d => d.FormField, o => o.Ignore())
                .ForMember(d => d.DependsOnField, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore());
        }
    }
}

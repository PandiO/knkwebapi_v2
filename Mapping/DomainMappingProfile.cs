using System.Collections.ObjectModel;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping;

public class DomainMappingProfile : Profile
{
    public DomainMappingProfile()
    {
        CreateMap<Domain, DomainDto>();
        CreateMap<DomainDto, Domain>();

        CreateMap<Domain, DomainListDto>();

        CreateMap<Domain, DomainRegionDecisionDto>()
            .ForMember(dest => dest.DomainType, opt => opt.MapFrom(src => src.GetType().Name))
            .ForMember(dest => dest.ParentDomainDecisions, opt => opt.MapFrom(src => GetParentDomainDecisions(src)));
    }

    private static Collection<DomainRegionDecisionDto> GetParentDomainDecisions(Domain domain)
    {
        var decisions = new Collection<DomainRegionDecisionDto>();
        var currentParentDomain = domain.ParentDomain;
        while (currentParentDomain != null)
        {
            decisions.Add(new DomainRegionDecisionDto
            {
                Id = currentParentDomain.Id,
                Name = currentParentDomain.Name,
                WgRegionId = currentParentDomain.WgRegionId,
                AllowEntry = currentParentDomain.AllowEntry,
                AllowExit = currentParentDomain.AllowExit,
                DomainType = currentParentDomain.GetType().Name
            });
            currentParentDomain = currentParentDomain.ParentDomain;
        }
        return decisions;
    }
}

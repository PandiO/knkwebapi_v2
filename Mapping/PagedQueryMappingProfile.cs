using System;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping;

public class PagedQueryMappingProfile : Profile
{
    public PagedQueryMappingProfile()
    {
        CreateMap<PagedQueryDto, PagedQuery>()
            .ForMember(dest => dest.PageNumber, src => src.MapFrom(src => src.PageNumber))
            .ForMember(dest => dest.PageSize, src => src.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.SearchTerm, src => src.MapFrom(src => src.SearchTerm))
            .ForMember(dest => dest.SortBy, src => src.MapFrom(src => src.SortBy))
            .ForMember(dest => dest.SortDescending, src => src.MapFrom(src => src.SortDescending))
            .ForMember(dest => dest.Filters, src => src.MapFrom(src => src.Filters));

        CreateMap<PagedQuery, PagedQueryDto>()
            .ForMember(dest => dest.PageNumber, src => src.MapFrom(src => src.PageNumber))
            .ForMember(dest => dest.PageSize, src => src.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.SearchTerm, src => src.MapFrom(src => src.SearchTerm))
            .ForMember(dest => dest.SortBy, src => src.MapFrom(src => src.SortBy))
            .ForMember(dest => dest.SortDescending, src => src.MapFrom(src => src.SortDescending))
            .ForMember(dest => dest.Filters, src => src.MapFrom(src => src.Filters));

        CreateMap(typeof(PagedResultDto<>), typeof(PagedResult<>));
        CreateMap(typeof(PagedResult<>), typeof(PagedResultDto<>));

    }
}

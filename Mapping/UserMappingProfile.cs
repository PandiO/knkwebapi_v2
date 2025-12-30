using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id, src => src.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, src => src.MapFrom(src => src.Username))
                .ForMember(dest => dest.Uuid, src => src.MapFrom(src => src.Uuid))
                .ForMember(dest => dest.Email, src => src.MapFrom(src => src.Email))
                .ForMember(dest => dest.Coins, src => src.MapFrom(src => src.Coins))
                .ForMember(dest => dest.CreatedAt, src => src.MapFrom(src => src.CreatedAt.ToString("O")));

            CreateMap<UserDto, User>()
                .ForMember(dest => dest.Id, src => src.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, src => src.MapFrom(src => src.Username))
                .ForMember(dest => dest.Uuid, src => src.MapFrom(src => src.Uuid))
                .ForMember(dest => dest.Email, src => src.MapFrom(src => src.Email))
                .ForMember(dest => dest.Coins, src => src.MapFrom(src => src.Coins))
                .ForMember(dest => dest.CreatedAt, src => src.MapFrom(src => DateTime.Parse(src.CreatedAt)))
                .ForMember(dest => dest.PasswordHash, src => src.Ignore());

            CreateMap<User, UserSummaryDto>()
                .ForMember(dest => dest.Id, src => src.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, src => src.MapFrom(src => src.Username))
                .ForMember(dest => dest.Uuid, src => src.MapFrom(src => src.Uuid))
                .ForMember(dest => dest.Coins, src => src.MapFrom(src => src.Coins));

            CreateMap<User, UserListDto>()
                .ForMember(dest => dest.id, src => src.MapFrom(src => src.Id))
                .ForMember(dest => dest.username, src => src.MapFrom(src => src.Username))
                .ForMember(dest => dest.uuid, src => src.MapFrom(src => src.Uuid))
                .ForMember(dest => dest.email, src => src.MapFrom(src => src.Email))
                .ForMember(dest => dest.Coins, src => src.MapFrom(src => src.Coins));
        
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.Id, src => src.Ignore())
                .ForMember(dest => dest.Username, src => src.MapFrom(src => src.Username))
                .ForMember(dest => dest.Uuid, src => src.MapFrom(src => src.Uuid))
                .ForMember(dest => dest.Email, src => src.MapFrom(src => src.Email))
                .ForMember(dest => dest.Coins, src => src.Ignore())
                .ForMember(dest => dest.CreatedAt, src => src.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.PasswordHash, src => src.Ignore());
        }


    }
}

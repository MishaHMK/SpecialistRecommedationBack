using AutoMapper;
using TherapyApp.Entities;
using TherapyApp.Helpers.Auth;
using TherapyApp.Helpers.Dto;

namespace TherapyApp.Helpers.Mapper;

public class UsersProfile : Profile
{
    public UsersProfile()
    {
        CreateMap<AppUser, Login>().ReverseMap();
        CreateMap<AppUser, Register>().ReverseMap();
        CreateMap<AppUser, UserDataDto>().ReverseMap();
    }
}
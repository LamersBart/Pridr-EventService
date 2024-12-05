// using EventService.DTOs;
// using EventService.Models;

using EventService.DTOs;
using EventService.Models;
using Profile = AutoMapper.Profile;

namespace EventService.Profiles;

public class UserEventsProfile : Profile
{
    public UserEventsProfile()
    {
        // source -> target
        CreateMap<UserEvent, UserEventReadDto>();
        CreateMap<UserEventUpdateDto, UserEvent>();
        CreateMap<UserEventCreateDto, UserEvent>();
    }
}
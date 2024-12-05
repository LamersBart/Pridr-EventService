using System.Text.Json;
using AutoMapper;
using EventService.Enums;
namespace EventService.EventProcessing;


public class EventProcessor : IEventProcessor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMapper _mapper;

    public EventProcessor(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _mapper = mapper;
    }
    
    public void ProcessEvent(string message)
    {
        Console.WriteLine($"--> Event received");
        var eventType = DetermineEvent(message);
        switch (eventType)
        {
            case EventType.Register:
                Console.WriteLine($"--> Event: {message}");
                AddUser(message);
                break;
            default:
                break;
        }
    }

    private EventType DetermineEvent(string notificationMessage)
    {
        Console.WriteLine("--> Determining Event");
        // var keycloakEvent = JsonSerializer.Deserialize<KeycloakEventDto>(notificationMessage);
        // switch (keycloakEvent.Type)
        // {
        //     case "LOGIN":
        //         Console.WriteLine("--> Login Event Detected");
        //         return EventType.Login;
        //     case "LOGOUT":
        //         Console.WriteLine("--> Logout Event Detected");
        //         return EventType.Logout;
        //     case "REGISTER":
        //         Console.WriteLine("--> Register Event Detected");
        //         return EventType.Register;
        //     default:
        //         Console.WriteLine("--> Other Event Detected");
        //         return EventType.Undetermined;
        // }
        return EventType.Undetermined;
    }

    private void AddUser(string keyCloakPublishedMessage)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            // var profileRepo = scope.ServiceProvider.GetRequiredService<IProfileRepo>();
            // var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepo>();
            // var keycloakEvent = JsonSerializer.Deserialize<KeycloakEventDto>(keyCloakPublishedMessage);
            // try
            // {
            //     var newUser = _mapper.Map<User>(keycloakEvent);
            //     if (!userRepo.UserExist(newUser.KeyCloakId))
            //     {
            //         userRepo.CreateUser(newUser);
            //         userRepo.SaveChanges();
            //         Profile newProfile = new Profile
            //         {
            //             UserId = newUser.Id,
            //             Sexuality = Sexuality.Unknown,
            //             LookingFor = LookingFor.Friendship,
            //             Latitude = 0,
            //             Longitude = 0,
            //             Weight = 0.0,
            //             Height = 0.0,
            //             RelationStatus = null,
            //             PartnerUserId = null,
            //             UserName = ""
            //         };
            //         profileRepo.CreateProfile(newProfile);
            //         profileRepo.SaveChanges();
            //         Console.WriteLine("--> User Added!");
            //     }
            //     else
            //     {
            //         Console.WriteLine($"--> Platform {newUser.KeyCloakId} already exists");
            //     }
            // }
            // catch (Exception ex)
            // {
            //     Console.WriteLine($"--> Could not add user to DB {ex.Message}");
            // }
        }
    }
}
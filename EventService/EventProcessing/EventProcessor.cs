using System.Text.Json;
using AutoMapper;
using EventService.Enums;
using EventService.DTOs;

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
            default:
                break;
        }
    }

    private static EventType DetermineEvent(string notificationMessage)
    {
        Console.WriteLine("--> Determining Event");
        var keycloakEvent = JsonSerializer.Deserialize<KeycloakEventDto>(notificationMessage);
        if (keycloakEvent != null)
        {
            switch (keycloakEvent.Type)
            {
                case "LOGIN":
                    Console.WriteLine("--> Login Event Detected");
                    return EventType.Login;
                case "LOGOUT":
                    Console.WriteLine("--> Logout Event Detected");
                    return EventType.Logout;
                case "REGISTER":
                    Console.WriteLine("--> Register Event Detected");
                    return EventType.Register;
                default:
                    Console.WriteLine("--> Other Event Detected");
                    return EventType.Undetermined;
            }
        }
        return EventType.Undetermined;
    }
}
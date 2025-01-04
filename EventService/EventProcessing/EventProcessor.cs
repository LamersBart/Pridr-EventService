using System.Text.Json;
using AutoMapper;
using EventService.Data;
using EventService.Enums;
using EventService.DTOs;

namespace EventService.EventProcessing;


public class EventProcessor : IEventProcessor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EventProcessor(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }
    
    public void ProcessEvent(string message)
    {
        Console.WriteLine($"--> Event received");
        var eventType = DetermineEvent(message);
        switch (eventType)
        {
            case EventType.Delete:
                Console.WriteLine($"--> Event: {message}");
                DeleteUserContent(message);
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
                case "DELETE_ACCOUNT":
                    Console.WriteLine("--> Delete account Event Detected");
                    return EventType.Delete;
                default:
                    Console.WriteLine("--> Other Event Detected");
                    return EventType.Undetermined;
            }
        }
        Console.WriteLine("--> Received Event is 'NULL'");
        return EventType.Undetermined;
    }
    
    private void DeleteUserContent(string keyCloakPublishedMessage)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var eventRepo = scope.ServiceProvider.GetRequiredService<IUserEventRepo>();
            var keycloakEvent = JsonSerializer.Deserialize<KeycloakEventDto>(keyCloakPublishedMessage);
            try
            {
                if (keycloakEvent != null)
                {
                    if (eventRepo.UserEventExistByUserId(keycloakEvent.UserId))
                    {
                        var eventsCreatedByUser = eventRepo.getAllUserEventsByUserId(keycloakEvent.UserId);
                        foreach (var userEvent in eventsCreatedByUser)
                        {
                            eventRepo.DeleteUserEvent(userEvent.Id);
                        }
                        eventRepo.SaveChanges();
                        Console.WriteLine("--> All Events created by the now deleted User are removed form the database!");
                    }
                    else
                    {
                        Console.WriteLine($"--> User {keycloakEvent.UserId} does not exist");
                    }
                }
                else
                {
                    Console.WriteLine($"--> Failed reading keycloak event {keyCloakPublishedMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not remove Events created by the now deleted User from DB {ex.Message}");
            }
        }
    }
}
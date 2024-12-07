using EventService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventService.Data;

public class UserEventRepo : IUserEventRepo
{
    private readonly AppDbContext _context;

    public UserEventRepo(AppDbContext context)
    {
        _context = context;
    }
    
    public bool SaveChanges()
    {
        return _context.SaveChanges() >= 0;
    }

    public IEnumerable<UserEvent> getAllUserEvents()
    {
        return _context.UserEvents.ToList();
    }

    public UserEvent GetUserEventById(int id)
    {
        return _context.UserEvents.FirstOrDefault(p => p.Id == id)!;
    }

    public void CreateUserEvent(UserEvent userEvent)
    {
        if (userEvent is null)
        {
            throw new ArgumentNullException(nameof(userEvent));
        }
        _context.UserEvents.Add(userEvent);
    }

    public bool UserEventExist(int userEventId)
    {
        return _context.UserEvents.Any(p => p.Id == userEventId);
    }

    public void UpdateUserEvent(UserEvent userEvent)
    {
        if (userEvent is null)
        {
            throw new ArgumentNullException(nameof(userEvent));
        }
        _context.UserEvents.Update(userEvent);
    }

    public void DeleteUserEvent(int userEventId)
    {
        UserEvent userEvent = GetUserEventById(userEventId);
        _context.UserEvents.Remove(userEvent);
    }
}
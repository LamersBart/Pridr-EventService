using EventService.Models;

namespace EventService.Data;

public interface IUserEventRepo
{
    bool SaveChanges();
    IEnumerable<UserEvent> getAllUserEvents();
    UserEvent GetUserEventById(int userEventId);
    void CreateUserEvent(UserEvent userEvent);
    bool UserEventExist(int userEventId);
    void UpdateUserEvent(UserEvent userEvent);
    void DeleteUserEvent(int userEventId);
}
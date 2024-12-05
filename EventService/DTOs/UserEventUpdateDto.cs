namespace EventService.DTOs;

public class UserEventUpdateDto
{
    public required string Name { get; set; }
    public required DateTime Date { get; set; }
}
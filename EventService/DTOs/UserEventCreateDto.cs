namespace EventService.DTOs;

public class UserEventCreateDto
{
    public required string Name { get; set; }
    public required DateTime Date { get; set; }
    public List<int>? ProfileIds { get; set; }
}
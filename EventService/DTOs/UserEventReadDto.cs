namespace EventService.DTOs;

public class UserEventReadDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required DateTime Date { get; set; }
    public IEnumerable<string>? ProfileIds { get; set; }
    public required DateTime CreatedOn { get; set; }
    public required string CreatedBy { get; set; }
}
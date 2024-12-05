using System.ComponentModel.DataAnnotations;

namespace EventService.Models;

public class UserEvent
{
    [Key]
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public required string Name { get; set; }
    
    [Required]
    public required DateTime Date { get; set; }
    
    [Required]
    public List<int>? ProfileIds { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string CreatedBy { get; set; }
    
    [Required]
    public DateTime CreatedOn { get; set; }
}
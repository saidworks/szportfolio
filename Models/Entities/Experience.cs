using System.ComponentModel.DataAnnotations;

namespace PortfolioCMS.Models.Entities;

public class Experience
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Company { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Position { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Location { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    public bool IsCurrent { get; set; } = false;
    
    public int DisplayOrder { get; set; }
}
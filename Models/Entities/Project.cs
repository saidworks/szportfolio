using System.ComponentModel.DataAnnotations;

namespace PortfolioCMS.Models.Entities;

public class Project
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? TechnologyStack { get; set; }
    
    [MaxLength(500)]
    public string? ProjectUrl { get; set; }
    
    [MaxLength(500)]
    public string? GitHubUrl { get; set; }
    
    [MaxLength(500)]
    public string? ImagePath { get; set; }
    
    public DateTime? CompletedDate { get; set; }
    
    public int DisplayOrder { get; set; }
}
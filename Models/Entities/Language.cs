using System.ComponentModel.DataAnnotations;

namespace PortfolioCMS.Models.Entities;

public class Language
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string ProficiencyLevel { get; set; } = string.Empty; // e.g., "Native", "Fluent", "Intermediate", "Basic"
    
    public int DisplayOrder { get; set; }
}
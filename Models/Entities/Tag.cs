using System.ComponentModel.DataAnnotations;

namespace PortfolioCMS.Models.Entities;

public class Tag
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Slug { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
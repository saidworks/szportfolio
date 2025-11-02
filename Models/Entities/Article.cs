using System.ComponentModel.DataAnnotations;

namespace PortfolioCMS.Models.Entities;

public class Article
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime? PublishedDate { get; set; }
    
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    
    [MaxLength(500)]
    public string? FeaturedImagePath { get; set; }
    
    // Navigation properties
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
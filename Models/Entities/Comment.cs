using System.ComponentModel.DataAnnotations;

namespace PortfolioCMS.Models.Entities;

public class Comment
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string AuthorName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string AuthorEmail { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
    
    public DateTime SubmittedDate { get; set; }
    
    public CommentStatus Status { get; set; } = CommentStatus.Pending;
    
    // Foreign key
    public int ArticleId { get; set; }
    
    // Navigation property
    public virtual Article Article { get; set; } = null!;
}
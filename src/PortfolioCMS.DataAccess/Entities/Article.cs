using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PortfolioCMS.DataAccess.Entities;

[Table("Articles")]
[Index(nameof(Status))]
[Index(nameof(PublishedDate))]
[Index(nameof(CreatedDate))]
[Index(nameof(UserId))]
public class Article
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    [Column(TypeName = "nvarchar(200)")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(500)]
    [Column(TypeName = "nvarchar(500)")]
    public string Summary { get; set; } = string.Empty;
    
    [Column(TypeName = "datetime2")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    [Column(TypeName = "datetime2")]
    public DateTime? PublishedDate { get; set; }
    
    [Required]
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    
    [MaxLength(500)]
    [Column(TypeName = "nvarchar(500)")]
    public string? FeaturedImageUrl { get; set; }
    
    [MaxLength(300)]
    [Column(TypeName = "nvarchar(300)")]
    public string? MetaDescription { get; set; }
    
    [MaxLength(200)]
    [Column(TypeName = "nvarchar(200)")]
    public string? MetaKeywords { get; set; }
    
    // Foreign key for AspNetUsers
    [MaxLength(450)]
    [Column(TypeName = "nvarchar(450)")]
    public string? UserId { get; set; }
    
    // Optimistic concurrency control
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public virtual AspNetUser? User { get; set; }
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
    public virtual ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PortfolioCMS.DataAccess.Entities;

[Table("Tags")]
[Index(nameof(Slug), IsUnique = true)]
[Index(nameof(Name))]
public class Tag
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    [Column(TypeName = "nvarchar(50)")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    [Column(TypeName = "nvarchar(50)")]
    public string Slug { get; set; } = string.Empty;
    
    [MaxLength(200)]
    [Column(TypeName = "nvarchar(200)")]
    public string? Description { get; set; }
    
    [Column(TypeName = "datetime2")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
    public virtual ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PortfolioCMS.DataAccess.Entities;

[Table("ArticleTags")]
[Index(nameof(ArticleId))]
[Index(nameof(TagId))]
public class ArticleTag
{
    [Key]
    public int Id { get; set; }
    
    public int ArticleId { get; set; }
    public int TagId { get; set; }
    
    [Column(TypeName = "datetime2")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Article Article { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}
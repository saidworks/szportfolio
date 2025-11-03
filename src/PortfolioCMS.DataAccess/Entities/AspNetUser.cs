using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PortfolioCMS.DataAccess.Entities;

[Table("AspNetUsers")]
[Index(nameof(Email))]
[Index(nameof(CreatedDate))]
public class AspNetUser : IdentityUser
{
    [MaxLength(100)]
    [Column(TypeName = "nvarchar(100)")]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    [Column(TypeName = "nvarchar(100)")]
    public string? LastName { get; set; }
    
    [Column(TypeName = "datetime2")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    [Column(TypeName = "datetime2")]
    public DateTime? LastLoginDate { get; set; }
    
    [MaxLength(50)]
    [Column(TypeName = "nvarchar(50)")]
    public string Role { get; set; } = "Viewer";
    
    // Navigation properties
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
    public virtual ICollection<MediaFile> UploadedMediaFiles { get; set; } = new List<MediaFile>();
}
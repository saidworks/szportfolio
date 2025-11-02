using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PortfolioCMS.Models.Entities;

public class User : IdentityUser
{
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginDate { get; set; }
    
    // Navigation properties
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
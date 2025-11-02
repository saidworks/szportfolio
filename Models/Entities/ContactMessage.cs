using System.ComponentModel.DataAnnotations;

namespace PortfolioCMS.Models.Entities;

public class ContactMessage
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Subject { get; set; }
    
    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
    
    public DateTime SubmittedDate { get; set; }
    
    public bool IsRead { get; set; } = false;
}
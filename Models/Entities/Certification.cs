using System.ComponentModel.DataAnnotations;

namespace PortfolioCMS.Models.Entities;

public class Certification
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string IssuingOrganization { get; set; } = string.Empty;
    
    public DateTime IssueDate { get; set; }
    
    public DateTime? ExpirationDate { get; set; }
    
    [MaxLength(500)]
    public string? CredentialId { get; set; }
    
    [MaxLength(500)]
    public string? CredentialUrl { get; set; }
    
    public int DisplayOrder { get; set; }
}
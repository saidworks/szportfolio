using System.ComponentModel.DataAnnotations;

namespace PortfolioCMS.Models.Entities;

public class MediaFile
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    public DateTime UploadedDate { get; set; }
    
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
}
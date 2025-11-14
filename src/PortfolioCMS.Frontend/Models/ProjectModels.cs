using System.ComponentModel.DataAnnotations;

namespace PortfolioCMS.Frontend.Models;

public class ProjectDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TechnologyStack { get; set; }
    public string? ProjectUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<MediaFileDto> MediaFiles { get; set; } = new();
}

public class CreateProjectDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? TechnologyStack { get; set; }

    [Url(ErrorMessage = "Invalid URL")]
    public string? ProjectUrl { get; set; }

    [Url(ErrorMessage = "Invalid URL")]
    public string? GitHubUrl { get; set; }

    public string? ImageUrl { get; set; }
    
    public DateTime? CompletedDate { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public class UpdateProjectDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? TechnologyStack { get; set; }

    [Url(ErrorMessage = "Invalid URL")]
    public string? ProjectUrl { get; set; }

    [Url(ErrorMessage = "Invalid URL")]
    public string? GitHubUrl { get; set; }

    public string? ImageUrl { get; set; }
    
    public DateTime? CompletedDate { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; }
}

public class MediaFileDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string BlobUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
}

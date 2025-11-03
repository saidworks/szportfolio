namespace PortfolioCMS.API.DTOs;

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
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TechnologyStack { get; set; }
    public string? ProjectUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateProjectDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TechnologyStack { get; set; }
    public string? ProjectUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
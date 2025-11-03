namespace PortfolioCMS.API.DTOs;

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public int ArticleCount { get; set; }
}

public class CreateTagDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateTagDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
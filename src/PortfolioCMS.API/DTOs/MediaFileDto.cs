namespace PortfolioCMS.API.DTOs;

public class MediaFileDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string BlobUrl { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateTime UploadedDate { get; set; }
    public string? UploadedBy { get; set; }
    public int? ArticleId { get; set; }
    public int? ProjectId { get; set; }
}

public class MediaUploadDto
{
    public IFormFile File { get; set; } = null!;
    public string? Category { get; set; }
    public int? ArticleId { get; set; }
    public int? ProjectId { get; set; }
}
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.API.DTOs;

// DTOs for testing - these will be implemented in the actual API project
public class ArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public ArticleStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public List<TagDto> Tags { get; set; } = new();
    public List<CommentDto> Comments { get; set; } = new();
}

public class CreateArticleDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<int> TagIds { get; set; } = new();
}

public class UpdateArticleDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<int> TagIds { get; set; } = new();
}

public class CommentDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public CommentStatus Status { get; set; }
    public DateTime SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
}

public class SubmitCommentDto
{
    public int ArticleId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class MediaFileDto
{
    public int Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; }
}

public class DashboardStatsDto
{
    public int TotalArticles { get; set; }
    public int PublishedArticles { get; set; }
    public int DraftArticles { get; set; }
    public int PendingComments { get; set; }
    public int TotalComments { get; set; }
}

public class BulkCommentActionDto
{
    public List<int> CommentIds { get; set; } = new();
}
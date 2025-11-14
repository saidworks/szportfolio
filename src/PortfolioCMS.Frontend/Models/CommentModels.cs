using System.ComponentModel.DataAnnotations;

namespace PortfolioCMS.Frontend.Models;

public enum CommentStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class CommentDto
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public CommentStatus Status { get; set; }
    public int ArticleId { get; set; }
    public string ArticleTitle { get; set; } = string.Empty;
}

public class CreateCommentDto
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string AuthorEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Comment is required")]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    public int ArticleId { get; set; }
}

public class CommentModerationDto
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public CommentStatus Status { get; set; }
    public int ArticleId { get; set; }
    public string ArticleTitle { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

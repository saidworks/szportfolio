using System.ComponentModel.DataAnnotations;

namespace PortfolioCMS.Frontend.Models;

public enum ArticleStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

public class ArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public ArticleStatus Status { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public List<TagDto> Tags { get; set; } = new();
    public int CommentCount { get; set; }
}

public class ArticleDetailDto : ArticleDto
{
    public string Content { get; set; } = string.Empty;
    public List<CommentDto> Comments { get; set; } = new();
}

public class CreateArticleDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Summary is required")]
    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;

    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    
    public string? FeaturedImageUrl { get; set; }
    
    [MaxLength(160)]
    public string? MetaDescription { get; set; }
    
    [MaxLength(255)]
    public string? MetaKeywords { get; set; }
    
    public List<string> TagNames { get; set; } = new();
}

public class UpdateArticleDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Summary is required")]
    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;

    public ArticleStatus Status { get; set; }
    
    public string? FeaturedImageUrl { get; set; }
    
    [MaxLength(160)]
    public string? MetaDescription { get; set; }
    
    [MaxLength(255)]
    public string? MetaKeywords { get; set; }
    
    public List<string> TagNames { get; set; } = new();
}

public class ArticleQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Tag { get; set; }
    public ArticleStatus? Status { get; set; }
    public string? SortBy { get; set; } = "CreatedDate";
    public string? SortOrder { get; set; } = "desc";
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ArticleCount { get; set; }
}

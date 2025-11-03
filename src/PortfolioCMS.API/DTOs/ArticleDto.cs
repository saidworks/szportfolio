using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.API.DTOs;

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
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    public string? FeaturedImageUrl { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public List<string> TagNames { get; set; } = new();
}

public class UpdateArticleDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public ArticleStatus Status { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? MetaDescription { get; set; }
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
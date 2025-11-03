using Microsoft.AspNetCore.Mvc;
using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.Services;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.API.Controllers;

// Controller classes for testing - these will be implemented in the actual API project
[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IContentService _contentService;
    private readonly ILogger<ArticlesController> _logger;

    public ArticlesController(IContentService contentService, ILogger<ArticlesController> logger)
    {
        _contentService = contentService;
        _logger = logger;
    }

    [HttpGet("published")]
    public async Task<ActionResult<IEnumerable<ArticleDto>>> GetPublishedArticles()
    {
        var articles = await _contentService.GetPublishedArticlesAsync();
        var articleDtos = articles.Select(MapToDto);
        return Ok(articleDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArticleDto>> GetArticleById(int id)
    {
        var article = await _contentService.GetArticleByIdAsync(id);
        if (article == null) return NotFound();
        return Ok(MapToDto(article));
    }

    [HttpPost]
    public async Task<ActionResult<ArticleDto>> CreateArticle(CreateArticleDto createDto)
    {
        var article = new Article
        {
            Title = createDto.Title,
            Content = createDto.Content,
            Summary = createDto.Summary,
            Status = ArticleStatus.Draft
        };

        var createdArticle = await _contentService.CreateArticleAsync(article);
        return CreatedAtAction(nameof(GetArticleById), new { id = createdArticle.Id }, MapToDto(createdArticle));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ArticleDto>> UpdateArticle(int id, UpdateArticleDto updateDto)
    {
        var article = new Article
        {
            Id = id,
            Title = updateDto.Title,
            Content = updateDto.Content,
            Summary = updateDto.Summary
        };

        var updatedArticle = await _contentService.UpdateArticleAsync(article);
        if (updatedArticle == null) return NotFound();
        return Ok(MapToDto(updatedArticle));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteArticle(int id)
    {
        var deleted = await _contentService.DeleteArticleAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ArticleDto>>> SearchArticles([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Search query cannot be empty");

        var articles = await _contentService.SearchArticlesAsync(query);
        var articleDtos = articles.Select(MapToDto);
        return Ok(articleDtos);
    }

    private static ArticleDto MapToDto(Article article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            Summary = article.Summary,
            Status = article.Status,
            CreatedDate = article.CreatedDate,
            PublishedDate = article.PublishedDate,
            ModifiedDate = article.ModifiedDate
        };
    }
}

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
    {
        _commentService = commentService;
        _logger = logger;
    }

    [HttpGet("article/{articleId}")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForArticle(int articleId)
    {
        var comments = await _commentService.GetApprovedCommentsForArticleAsync(articleId);
        var commentDtos = comments.Select(MapToDto);
        return Ok(commentDtos);
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> SubmitComment(SubmitCommentDto submitDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var comment = new Comment
        {
            ArticleId = submitDto.ArticleId,
            AuthorName = submitDto.AuthorName,
            AuthorEmail = submitDto.AuthorEmail,
            Content = submitDto.Content
        };

        var submittedComment = await _commentService.SubmitCommentAsync(comment);
        return CreatedAtAction(nameof(GetCommentsForArticle), new { articleId = submittedComment.ArticleId }, MapToDto(submittedComment));
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetPendingComments()
    {
        var comments = await _commentService.GetPendingCommentsAsync();
        var commentDtos = comments.Select(MapToDto);
        return Ok(commentDtos);
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult> ApproveComment(int id)
    {
        var approved = await _commentService.ApproveCommentAsync(id);
        if (!approved) return NotFound();
        return Ok(new { message = "Comment approved successfully" });
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult> RejectComment(int id)
    {
        var rejected = await _commentService.RejectCommentAsync(id);
        if (!rejected) return NotFound();
        return Ok(new { message = "Comment rejected successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteComment(int id)
    {
        var deleted = await _commentService.DeleteCommentAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    private static CommentDto MapToDto(Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            ArticleId = comment.ArticleId,
            AuthorName = comment.AuthorName,
            AuthorEmail = comment.AuthorEmail,
            Content = comment.Content,
            Status = comment.Status,
            SubmittedDate = comment.SubmittedDate,
            ApprovedDate = comment.ApprovedDate
        };
    }
}

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IContentService _contentService;
    private readonly ICommentService _commentService;
    private readonly IMediaService _mediaService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IContentService contentService,
        ICommentService commentService,
        IMediaService mediaService,
        ILogger<AdminController> logger)
    {
        _contentService = contentService;
        _commentService = commentService;
        _mediaService = mediaService;
        _logger = logger;
    }

    [HttpGet("articles")]
    public async Task<ActionResult<IEnumerable<ArticleDto>>> GetAllArticles()
    {
        var articles = await _contentService.GetAllArticlesAsync();
        var articleDtos = articles.Select(MapToArticleDto);
        return Ok(articleDtos);
    }

    [HttpPost("articles/{id}/publish")]
    public async Task<ActionResult<ArticleDto>> PublishArticle(int id)
    {
        var article = await _contentService.PublishArticleAsync(id);
        if (article == null) return NotFound();
        return Ok(MapToArticleDto(article));
    }

    [HttpPost("articles/{id}/unpublish")]
    public async Task<ActionResult<ArticleDto>> UnpublishArticle(int id)
    {
        var article = await _contentService.UnpublishArticleAsync(id);
        if (article == null) return NotFound();
        return Ok(MapToArticleDto(article));
    }

    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        var stats = await _contentService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    [HttpPost("comments/bulk-approve")]
    public async Task<ActionResult> BulkApproveComments(BulkCommentActionDto request)
    {
        var count = await _commentService.BulkApproveCommentsAsync(request.CommentIds);
        return Ok(new { approvedCount = count });
    }

    [HttpPost("comments/bulk-reject")]
    public async Task<ActionResult> BulkRejectComments(BulkCommentActionDto request)
    {
        var count = await _commentService.BulkRejectCommentsAsync(request.CommentIds);
        return Ok(new { rejectedCount = count });
    }

    [HttpPost("comments/bulk-delete")]
    public async Task<ActionResult> BulkDeleteComments(BulkCommentActionDto request)
    {
        var count = await _commentService.BulkDeleteCommentsAsync(request.CommentIds);
        return Ok(new { deletedCount = count });
    }

    [HttpGet("media")]
    public async Task<ActionResult<IEnumerable<MediaFileDto>>> GetMediaFiles()
    {
        var files = await _mediaService.GetAllMediaFilesAsync();
        var fileDtos = files.Select(MapToMediaFileDto);
        return Ok(fileDtos);
    }

    private static ArticleDto MapToArticleDto(Article article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            Summary = article.Summary,
            Status = article.Status,
            CreatedDate = article.CreatedDate,
            PublishedDate = article.PublishedDate,
            ModifiedDate = article.ModifiedDate
        };
    }

    private static MediaFileDto MapToMediaFileDto(MediaFile file)
    {
        return new MediaFileDto
        {
            Id = file.Id,
            OriginalFileName = file.OriginalFileName,
            StoredFileName = file.StoredFileName,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            UploadDate = file.UploadDate
        };
    }
}

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IObservabilityService _observabilityService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IObservabilityService observabilityService, ILogger<HealthController> logger)
    {
        _observabilityService = observabilityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetHealth()
    {
        var health = await _observabilityService.GetHealthStatusAsync();
        var status = health.GetType().GetProperty("Status")?.GetValue(health)?.ToString();
        
        if (status == "Healthy")
            return Ok(health);
        else
            return StatusCode(503, health); // Service Unavailable
    }

    [HttpGet("ready")]
    public async Task<ActionResult> GetReadiness()
    {
        var readiness = await _observabilityService.GetReadinessStatusAsync();
        var status = readiness.GetType().GetProperty("Status")?.GetValue(readiness)?.ToString();
        
        if (status == "Ready")
            return Ok(readiness);
        else
            return StatusCode(503, readiness); // Service Unavailable
    }

    [HttpGet("live")]
    public async Task<ActionResult> GetLiveness()
    {
        var liveness = await _observabilityService.GetLivenessStatusAsync();
        return Ok(liveness);
    }

    [HttpGet("metrics")]
    public async Task<ActionResult> GetMetrics()
    {
        var metrics = await _observabilityService.GetMetricsAsync();
        return Ok(metrics);
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortfolioCMS.API.DTOs;
using PortfolioCMS.DataAccess.Context;
using PortfolioCMS.DataAccess.Entities;
using PortfolioCMS.DataAccess.Repositories;

namespace PortfolioCMS.API.Services;

// Service interfaces for testing - these will be implemented in the actual API project
public interface IContentService
{
    Task<IEnumerable<Article>> GetPublishedArticlesAsync();
    Task<IEnumerable<Article>> GetAllArticlesAsync();
    Task<Article?> GetArticleByIdAsync(int id);
    Task<Article> CreateArticleAsync(Article article);
    Task<Article?> UpdateArticleAsync(Article article);
    Task<bool> DeleteArticleAsync(int id);
    Task<IEnumerable<Article>> SearchArticlesAsync(string query);
    Task<Article?> PublishArticleAsync(int id);
    Task<Article?> UnpublishArticleAsync(int id);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}

public interface ICommentService
{
    Task<IEnumerable<Comment>> GetApprovedCommentsForArticleAsync(int articleId);
    Task<IEnumerable<Comment>> GetPendingCommentsAsync();
    Task<Comment> SubmitCommentAsync(Comment comment);
    Task<bool> ApproveCommentAsync(int commentId);
    Task<bool> RejectCommentAsync(int commentId);
    Task<bool> DeleteCommentAsync(int commentId);
    Task<int> BulkApproveCommentsAsync(IEnumerable<int> commentIds);
    Task<int> BulkRejectCommentsAsync(IEnumerable<int> commentIds);
    Task<int> BulkDeleteCommentsAsync(IEnumerable<int> commentIds);
}

public interface IMediaService
{
    Task<MediaFile> UploadFileAsync(IFormFile file);
    Task<MediaFile?> GetMediaFileAsync(int id);
    Task<IEnumerable<MediaFile>> GetAllMediaFilesAsync();
    Task<bool> DeleteMediaFileAsync(int id);
    bool IsValidImageType(string contentType);
}

public interface IObservabilityService
{
    Task<object> GetHealthStatusAsync();
    Task<object> GetReadinessStatusAsync();
    Task<object> GetLivenessStatusAsync();
    Task<object> GetMetricsAsync();
    void LogCustomEvent(string eventName, Dictionary<string, object> properties);
    void LogCustomMetric(string metricName, double value, Dictionary<string, string>? properties = null);
    void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success);
}

// Concrete implementations for testing
public class ContentService : IContentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ContentService> _logger;

    public ContentService(IUnitOfWork unitOfWork, ILogger<ContentService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Article>> GetPublishedArticlesAsync()
    {
        return await _unitOfWork.Articles.GetPublishedArticlesAsync();
    }

    public async Task<IEnumerable<Article>> GetAllArticlesAsync()
    {
        return await _unitOfWork.Articles.GetAllAsync();
    }

    public async Task<Article?> GetArticleByIdAsync(int id)
    {
        return await _unitOfWork.Articles.GetByIdAsync(id);
    }

    public async Task<Article> CreateArticleAsync(Article article)
    {
        article.CreatedDate = DateTime.UtcNow;
        await _unitOfWork.Articles.AddAsync(article);
        await _unitOfWork.SaveChangesAsync();
        return article;
    }

    public async Task<Article?> UpdateArticleAsync(Article article)
    {
        var existing = await _unitOfWork.Articles.GetByIdAsync(article.Id);
        if (existing == null) return null;

        existing.Title = article.Title;
        existing.Content = article.Content;
        existing.Summary = article.Summary;
        // Note: ModifiedDate would be handled by the entity if it exists

        await _unitOfWork.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteArticleAsync(int id)
    {
        var article = await _unitOfWork.Articles.GetByIdAsync(id);
        if (article == null) return false;

        await _unitOfWork.Articles.DeleteAsync(article);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Article>> SearchArticlesAsync(string query)
    {
        return await _unitOfWork.Articles.SearchAsync(query);
    }

    public async Task<Article?> PublishArticleAsync(int id)
    {
        var article = await _unitOfWork.Articles.GetByIdAsync(id);
        if (article == null) return null;

        article.Status = ArticleStatus.Published;
        article.PublishedDate = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return article;
    }

    public async Task<Article?> UnpublishArticleAsync(int id)
    {
        var article = await _unitOfWork.Articles.GetByIdAsync(id);
        if (article == null) return null;

        article.Status = ArticleStatus.Draft;
        article.PublishedDate = null;
        await _unitOfWork.SaveChangesAsync();
        return article;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var allArticles = await _unitOfWork.Articles.GetAllAsync();
        var allComments = await _unitOfWork.Comments.GetAllAsync();

        return new DashboardStatsDto
        {
            TotalArticles = allArticles.Count(),
            PublishedArticles = allArticles.Count(a => a.Status == ArticleStatus.Published),
            DraftArticles = allArticles.Count(a => a.Status == ArticleStatus.Draft),
            PendingComments = allComments.Count(c => c.Status == CommentStatus.Pending),
            TotalComments = allComments.Count()
        };
    }
}

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CommentService> _logger;

    public CommentService(IUnitOfWork unitOfWork, ILogger<CommentService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Comment>> GetApprovedCommentsForArticleAsync(int articleId)
    {
        return await _unitOfWork.Comments.GetApprovedCommentsForArticleAsync(articleId);
    }

    public async Task<IEnumerable<Comment>> GetPendingCommentsAsync()
    {
        return await _unitOfWork.Comments.GetPendingCommentsAsync();
    }

    public async Task<Comment> SubmitCommentAsync(Comment comment)
    {
        if (string.IsNullOrWhiteSpace(comment.Content))
            throw new ArgumentException("Comment content cannot be empty");

        comment.Status = CommentStatus.Pending;
        comment.SubmittedDate = DateTime.UtcNow;
        await _unitOfWork.Comments.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();
        return comment;
    }

    public async Task<bool> ApproveCommentAsync(int commentId)
    {
        var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
        if (comment == null) return false;

        comment.Status = CommentStatus.Approved;
        // Note: ApprovedDate would be handled by the entity if it exists
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectCommentAsync(int commentId)
    {
        var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
        if (comment == null) return false;

        comment.Status = CommentStatus.Rejected;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
        if (comment == null) return false;

        await _unitOfWork.Comments.DeleteAsync(comment);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<int> BulkApproveCommentsAsync(IEnumerable<int> commentIds)
    {
        int count = 0;
        foreach (var id in commentIds)
        {
            if (await ApproveCommentAsync(id))
                count++;
        }
        return count;
    }

    public async Task<int> BulkRejectCommentsAsync(IEnumerable<int> commentIds)
    {
        int count = 0;
        foreach (var id in commentIds)
        {
            if (await RejectCommentAsync(id))
                count++;
        }
        return count;
    }

    public async Task<int> BulkDeleteCommentsAsync(IEnumerable<int> commentIds)
    {
        int count = 0;
        foreach (var id in commentIds)
        {
            if (await DeleteCommentAsync(id))
                count++;
        }
        return count;
    }
}

public class MediaService : IMediaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MediaService> _logger;
    private readonly string[] _allowedImageTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public MediaService(IUnitOfWork unitOfWork, ILogger<MediaService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<MediaFile> UploadFileAsync(IFormFile file)
    {
        if (!IsValidImageType(file.ContentType))
            throw new ArgumentException("Invalid file type");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("File too large");

        var mediaFile = new MediaFile
        {
            OriginalFileName = file.FileName,
            FileName = $"{Guid.NewGuid()}_{file.FileName}",
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedDate = DateTime.UtcNow
        };

        await _unitOfWork.GetRepository<MediaFile>().AddAsync(mediaFile);
        await _unitOfWork.SaveChangesAsync();
        return mediaFile;
    }

    public async Task<MediaFile?> GetMediaFileAsync(int id)
    {
        return await _unitOfWork.GetRepository<MediaFile>().GetByIdAsync(id);
    }

    public async Task<IEnumerable<MediaFile>> GetAllMediaFilesAsync()
    {
        return await _unitOfWork.GetRepository<MediaFile>().GetAllAsync();
    }

    public async Task<bool> DeleteMediaFileAsync(int id)
    {
        var file = await _unitOfWork.GetRepository<MediaFile>().GetByIdAsync(id);
        if (file == null) return false;

        await _unitOfWork.GetRepository<MediaFile>().DeleteAsync(file);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public bool IsValidImageType(string contentType)
    {
        return _allowedImageTypes.Contains(contentType);
    }
}

public class ObservabilityService : IObservabilityService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ObservabilityService> _logger;

    public ObservabilityService(ApplicationDbContext dbContext, ILogger<ObservabilityService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<object> GetHealthStatusAsync()
    {
        var isHealthy = await _dbContext.Database.CanConnectAsync();
        return new
        {
            Status = isHealthy ? "Healthy" : "Unhealthy",
            Timestamp = DateTime.UtcNow,
            Database = isHealthy ? "Healthy" : "Unhealthy",
            Dependencies = new { }
        };
    }

    public async Task<object> GetReadinessStatusAsync()
    {
        var canConnect = await _dbContext.Database.CanConnectAsync();
        // For testing, assume no pending migrations
        var hasPendingMigrations = false;


        return new
        {
            Status = canConnect && !hasPendingMigrations ? "Ready" : "NotReady",
            Timestamp = DateTime.UtcNow,
            Database = canConnect ? "Connected" : "Disconnected",
            Migrations = hasPendingMigrations ? "Pending" : "Applied"
        };
    }

    public async Task<object> GetLivenessStatusAsync()
    {
        return await Task.FromResult(new
        {
            Status = "Alive",
            Timestamp = DateTime.UtcNow,
            Uptime = TimeSpan.FromMilliseconds(Environment.TickCount64)
        });
    }

    public async Task<object> GetMetricsAsync()
    {
        return await Task.FromResult(new
        {
            RequestCount = 0,
            AverageResponseTime = 0.0,
            ErrorRate = 0.0,
            ActiveConnections = 0,
            MemoryUsage = GC.GetTotalMemory(false) / 1024.0 / 1024.0, // MB
            CpuUsage = 0.0
        });
    }

    public void LogCustomEvent(string eventName, Dictionary<string, object> properties)
    {
        _logger.LogInformation("Custom Event: {EventName} with properties: {@Properties}", eventName, properties);
    }

    public void LogCustomMetric(string metricName, double value, Dictionary<string, string>? properties = null)
    {
        _logger.LogInformation("Custom Metric: {MetricName} = {Value} with properties: {@Properties}", metricName, value, properties);
    }

    public void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success)
    {
        _logger.LogInformation("Dependency: {DependencyName}.{CommandName} took {Duration}ms, Success: {Success}", 
            dependencyName, commandName, duration.TotalMilliseconds, success);
    }
}
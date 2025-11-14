using PortfolioCMS.Frontend.Models;

namespace PortfolioCMS.Frontend.Services;

public class CommentService : ICommentService
{
    private readonly IApiService _apiService;
    private readonly ILogger<CommentService> _logger;

    public CommentService(
        IApiService apiService,
        ILogger<CommentService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<CommentDto?> SubmitCommentAsync(CreateCommentDto dto)
    {
        try
        {
            return await _apiService.PostAsync<CreateCommentDto, CommentDto>("/api/v1/comments", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting comment");
            return null;
        }
    }

    public async Task<List<CommentModerationDto>?> GetPendingCommentsAsync()
    {
        try
        {
            return await _apiService.GetAsync<List<CommentModerationDto>>("/api/v1/comments/pending");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending comments");
            return null;
        }
    }

    public async Task<bool> ApproveCommentAsync(int commentId)
    {
        try
        {
            var result = await _apiService.PostAsync<object>($"/api/v1/comments/{commentId}/approve");
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving comment {CommentId}", commentId);
            return false;
        }
    }

    public async Task<bool> RejectCommentAsync(int commentId)
    {
        try
        {
            var result = await _apiService.PostAsync<object>($"/api/v1/comments/{commentId}/reject");
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting comment {CommentId}", commentId);
            return false;
        }
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        try
        {
            return await _apiService.DeleteAsync($"/api/v1/comments/{commentId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return false;
        }
    }
}

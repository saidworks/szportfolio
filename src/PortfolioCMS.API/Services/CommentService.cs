using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.DTOs.Common;
using PortfolioCMS.DataAccess.Entities;
using PortfolioCMS.DataAccess.Repositories;

namespace PortfolioCMS.API.Services;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObservabilityService _observability;
    private readonly ILogger<CommentService> _logger;

    public CommentService(
        IUnitOfWork unitOfWork,
        IObservabilityService observability,
        ILogger<CommentService> logger)
    {
        _unitOfWork = unitOfWork;
        _observability = observability;
        _logger = logger;
    }

    public async Task<ApiResponse<CommentDto>> SubmitCommentAsync(CreateCommentDto dto, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            using var operation = _observability.StartOperation("SubmitComment");

            // Verify article exists
            var article = await _unitOfWork.Articles.GetByIdAsync(dto.ArticleId);
            if (article == null)
            {
                return new ApiResponse<CommentDto>
                {
                    Success = false,
                    Message = "Article not found"
                };
            }

            var comment = new Comment
            {
                AuthorName = dto.AuthorName,
                AuthorEmail = dto.AuthorEmail,
                Content = dto.Content,
                ArticleId = dto.ArticleId,
                SubmittedDate = DateTime.UtcNow,
                Status = CommentStatus.Pending,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            var createdComment = await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("CommentSubmitted", new Dictionary<string, string>
            {
                ["CommentId"] = createdComment.Id.ToString(),
                ["ArticleId"] = dto.ArticleId.ToString(),
                ["AuthorEmail"] = dto.AuthorEmail
            });

            return new ApiResponse<CommentDto>
            {
                Success = true,
                Data = MapToCommentDto(createdComment),
                Message = "Comment submitted successfully and is pending moderation"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(SubmitCommentAsync),
                ["ArticleId"] = dto.ArticleId.ToString()
            });

            return new ApiResponse<CommentDto>
            {
                Success = false,
                Message = "Failed to submit comment",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<List<CommentDto>> GetApprovedCommentsAsync(int articleId)
    {
        try
        {
            var comments = await _unitOfWork.Comments.GetApprovedCommentsByArticleIdAsync(articleId);
            return comments.Select(MapToCommentDto).ToList();
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetApprovedCommentsAsync),
                ["ArticleId"] = articleId.ToString()
            });
            throw;
        }
    }

    public async Task<PagedResult<CommentModerationDto>> GetPendingCommentsAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            var comments = await _unitOfWork.Comments.GetCommentsByStatusAsync(CommentStatus.Pending);
            var totalCount = comments.Count();

            var pagedComments = comments
                .OrderByDescending(c => c.SubmittedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<CommentModerationDto>
            {
                Items = pagedComments.Select(MapToCommentModerationDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetPendingCommentsAsync)
            });
            throw;
        }
    }

    public async Task<PagedResult<CommentModerationDto>> GetAllCommentsAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            var comments = await _unitOfWork.Comments.GetAllAsync();
            var totalCount = comments.Count();

            var pagedComments = comments
                .OrderByDescending(c => c.SubmittedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<CommentModerationDto>
            {
                Items = pagedComments.Select(MapToCommentModerationDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetAllCommentsAsync)
            });
            throw;
        }
    }

    public async Task<ApiResponse<CommentDto>> ApproveCommentAsync(int commentId, string userId)
    {
        try
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
            if (comment == null)
            {
                return new ApiResponse<CommentDto>
                {
                    Success = false,
                    Message = "Comment not found"
                };
            }

            comment.Status = CommentStatus.Approved;
            await _unitOfWork.Comments.UpdateAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("CommentApproved", new Dictionary<string, string>
            {
                ["CommentId"] = commentId.ToString(),
                ["ArticleId"] = comment.ArticleId.ToString(),
                ["ModeratorId"] = userId
            });

            return new ApiResponse<CommentDto>
            {
                Success = true,
                Data = MapToCommentDto(comment),
                Message = "Comment approved successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(ApproveCommentAsync),
                ["CommentId"] = commentId.ToString()
            });

            return new ApiResponse<CommentDto>
            {
                Success = false,
                Message = "Failed to approve comment",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> RejectCommentAsync(int commentId, string userId)
    {
        try
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
            if (comment == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Comment not found"
                };
            }

            comment.Status = CommentStatus.Rejected;
            await _unitOfWork.Comments.UpdateAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("CommentRejected", new Dictionary<string, string>
            {
                ["CommentId"] = commentId.ToString(),
                ["ArticleId"] = comment.ArticleId.ToString(),
                ["ModeratorId"] = userId
            });

            return new ApiResponse
            {
                Success = true,
                Message = "Comment rejected successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(RejectCommentAsync),
                ["CommentId"] = commentId.ToString()
            });

            return new ApiResponse
            {
                Success = false,
                Message = "Failed to reject comment",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> DeleteCommentAsync(int commentId, string userId)
    {
        try
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
            if (comment == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Comment not found"
                };
            }

            await _unitOfWork.Comments.DeleteAsync(commentId);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("CommentDeleted", new Dictionary<string, string>
            {
                ["CommentId"] = commentId.ToString(),
                ["ArticleId"] = comment.ArticleId.ToString(),
                ["ModeratorId"] = userId
            });

            return new ApiResponse
            {
                Success = true,
                Message = "Comment deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(DeleteCommentAsync),
                ["CommentId"] = commentId.ToString()
            });

            return new ApiResponse
            {
                Success = false,
                Message = "Failed to delete comment",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> BulkApproveCommentsAsync(List<int> commentIds, string userId)
    {
        try
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            var approvedCount = 0;
            foreach (var commentId in commentIds)
            {
                var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
                if (comment != null)
                {
                    comment.Status = CommentStatus.Approved;
                    await _unitOfWork.Comments.UpdateAsync(comment);
                    approvedCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _observability.TrackEvent("CommentsBulkApproved", new Dictionary<string, string>
            {
                ["CommentCount"] = approvedCount.ToString(),
                ["ModeratorId"] = userId
            });

            return new ApiResponse
            {
                Success = true,
                Message = $"{approvedCount} comments approved successfully"
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(BulkApproveCommentsAsync)
            });

            return new ApiResponse
            {
                Success = false,
                Message = "Failed to approve comments",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> BulkRejectCommentsAsync(List<int> commentIds, string userId)
    {
        try
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            var rejectedCount = 0;
            foreach (var commentId in commentIds)
            {
                var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
                if (comment != null)
                {
                    comment.Status = CommentStatus.Rejected;
                    await _unitOfWork.Comments.UpdateAsync(comment);
                    rejectedCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _observability.TrackEvent("CommentsBulkRejected", new Dictionary<string, string>
            {
                ["CommentCount"] = rejectedCount.ToString(),
                ["ModeratorId"] = userId
            });

            return new ApiResponse
            {
                Success = true,
                Message = $"{rejectedCount} comments rejected successfully"
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(BulkRejectCommentsAsync)
            });

            return new ApiResponse
            {
                Success = false,
                Message = "Failed to reject comments",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> BulkDeleteCommentsAsync(List<int> commentIds, string userId)
    {
        try
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            var deletedCount = 0;
            foreach (var commentId in commentIds)
            {
                var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
                if (comment != null)
                {
                    await _unitOfWork.Comments.DeleteAsync(commentId);
                    deletedCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _observability.TrackEvent("CommentsBulkDeleted", new Dictionary<string, string>
            {
                ["CommentCount"] = deletedCount.ToString(),
                ["ModeratorId"] = userId
            });

            return new ApiResponse
            {
                Success = true,
                Message = $"{deletedCount} comments deleted successfully"
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(BulkDeleteCommentsAsync)
            });

            return new ApiResponse
            {
                Success = false,
                Message = "Failed to delete comments",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<CommentStatisticsDto> GetCommentStatisticsAsync()
    {
        try
        {
            var allComments = await _unitOfWork.Comments.GetAllAsync();
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var statistics = new CommentStatisticsDto
            {
                TotalComments = allComments.Count(),
                PendingComments = allComments.Count(c => c.Status == CommentStatus.Pending),
                ApprovedComments = allComments.Count(c => c.Status == CommentStatus.Approved),
                RejectedComments = allComments.Count(c => c.Status == CommentStatus.Rejected),
                CommentsToday = allComments.Count(c => c.SubmittedDate.Date == today),
                CommentsThisWeek = allComments.Count(c => c.SubmittedDate.Date >= weekStart),
                CommentsThisMonth = allComments.Count(c => c.SubmittedDate.Date >= monthStart)
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetCommentStatisticsAsync)
            });
            throw;
        }
    }

    private static CommentDto MapToCommentDto(Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            AuthorName = comment.AuthorName,
            AuthorEmail = comment.AuthorEmail,
            Content = comment.Content,
            SubmittedDate = comment.SubmittedDate,
            Status = comment.Status,
            ArticleId = comment.ArticleId,
            ArticleTitle = comment.Article?.Title ?? string.Empty
        };
    }

    private static CommentModerationDto MapToCommentModerationDto(Comment comment)
    {
        return new CommentModerationDto
        {
            Id = comment.Id,
            AuthorName = comment.AuthorName,
            AuthorEmail = comment.AuthorEmail,
            Content = comment.Content,
            SubmittedDate = comment.SubmittedDate,
            Status = comment.Status,
            ArticleId = comment.ArticleId,
            ArticleTitle = comment.Article?.Title ?? string.Empty,
            IpAddress = comment.IpAddress,
            UserAgent = comment.UserAgent
        };
    }
}
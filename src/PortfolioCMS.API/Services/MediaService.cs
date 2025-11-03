using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.DTOs.Common;
using PortfolioCMS.DataAccess.Entities;
using PortfolioCMS.DataAccess.Repositories;

namespace PortfolioCMS.API.Services;

public class MediaService : IMediaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObservabilityService _observability;
    private readonly ILogger<MediaService> _logger;
    private readonly IConfiguration _configuration;

    // Allowed file types and sizes
    private readonly string[] _allowedImageTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
    private readonly string[] _allowedDocumentTypes = { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

    public MediaService(
        IUnitOfWork unitOfWork,
        IObservabilityService observability,
        ILogger<MediaService> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _observability = observability;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<ApiResponse<MediaFileDto>> UploadMediaAsync(MediaUploadDto dto, string userId)
    {
        try
        {
            using var operation = _observability.StartOperation("UploadMedia");

            // Validate file
            var validationResult = await ValidateFileAsync(dto.File);
            if (!validationResult.Success)
            {
                return new ApiResponse<MediaFileDto>
                {
                    Success = false,
                    Message = validationResult.Message,
                    Errors = validationResult.Errors
                };
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(dto.File.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            // For now, we'll simulate blob storage by saving to a local directory
            // In production, this would upload to Azure Blob Storage
            var uploadsPath = Path.Combine("wwwroot", "uploads");
            Directory.CreateDirectory(uploadsPath);
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var mediaFile = new MediaFile
            {
                FileName = uniqueFileName,
                OriginalFileName = dto.File.FileName ?? "unknown",
                ContentType = dto.File.ContentType ?? "application/octet-stream",
                FileSize = dto.File.Length,
                BlobUrl = $"/uploads/{uniqueFileName}", // In production, this would be the Azure Blob URL
                Category = dto.Category ?? string.Empty,
                UploadedDate = DateTime.UtcNow,
                UploadedBy = userId,
                ArticleId = dto.ArticleId,
                ProjectId = dto.ProjectId
            };

            var createdMediaFile = await _unitOfWork.MediaFiles.AddAsync(mediaFile);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("MediaFileUploaded", new Dictionary<string, string>
            {
                ["MediaFileId"] = createdMediaFile.Id.ToString(),
                ["FileName"] = createdMediaFile.OriginalFileName,
                ["FileSize"] = createdMediaFile.FileSize.ToString(),
                ["ContentType"] = createdMediaFile.ContentType,
                ["Category"] = createdMediaFile.Category ?? "Unknown"
            });

            return new ApiResponse<MediaFileDto>
            {
                Success = true,
                Data = MapToMediaFileDto(createdMediaFile),
                Message = "File uploaded successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(UploadMediaAsync)
            });

            return new ApiResponse<MediaFileDto>
            {
                Success = false,
                Message = "Failed to upload file",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<PagedResult<MediaFileDto>> GetMediaFilesAsync(int page = 1, int pageSize = 20, string? category = null)
    {
        try
        {
            var mediaFiles = await _unitOfWork.MediaFiles.GetAllAsync();

            if (!string.IsNullOrEmpty(category))
            {
                mediaFiles = mediaFiles.Where(m => m.Category == category);
            }

            var totalCount = mediaFiles.Count();
            var pagedFiles = mediaFiles
                .OrderByDescending(m => m.UploadedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<MediaFileDto>
            {
                Items = pagedFiles.Select(MapToMediaFileDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetMediaFilesAsync)
            });
            throw;
        }
    }

    public async Task<MediaFileDto?> GetMediaFileByIdAsync(int id)
    {
        try
        {
            var mediaFile = await _unitOfWork.MediaFiles.GetByIdAsync(id);
            return mediaFile != null ? MapToMediaFileDto(mediaFile) : null;
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetMediaFileByIdAsync),
                ["MediaFileId"] = id.ToString()
            });
            throw;
        }
    }

    public async Task<ApiResponse> DeleteMediaFileAsync(int id, string userId)
    {
        try
        {
            var mediaFile = await _unitOfWork.MediaFiles.GetByIdAsync(id);
            if (mediaFile == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Media file not found"
                };
            }

            // Delete physical file
            var filePath = Path.Combine("wwwroot", "uploads", mediaFile.FileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            await _unitOfWork.MediaFiles.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("MediaFileDeleted", new Dictionary<string, string>
            {
                ["MediaFileId"] = id.ToString(),
                ["FileName"] = mediaFile.OriginalFileName,
                ["DeletedBy"] = userId
            });

            return new ApiResponse
            {
                Success = true,
                Message = "Media file deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(DeleteMediaFileAsync),
                ["MediaFileId"] = id.ToString()
            });

            return new ApiResponse
            {
                Success = false,
                Message = "Failed to delete media file",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<MediaFileDto>> UpdateMediaFileAsync(int id, UpdateMediaFileDto dto, string userId)
    {
        try
        {
            var mediaFile = await _unitOfWork.MediaFiles.GetByIdAsync(id);
            if (mediaFile == null)
            {
                return new ApiResponse<MediaFileDto>
                {
                    Success = false,
                    Message = "Media file not found"
                };
            }

            mediaFile.Category = dto.Category ?? string.Empty;
            mediaFile.ArticleId = dto.ArticleId;
            mediaFile.ProjectId = dto.ProjectId;

            await _unitOfWork.MediaFiles.UpdateAsync(mediaFile);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("MediaFileUpdated", new Dictionary<string, string>
            {
                ["MediaFileId"] = id.ToString(),
                ["UpdatedBy"] = userId
            });

            return new ApiResponse<MediaFileDto>
            {
                Success = true,
                Data = MapToMediaFileDto(mediaFile),
                Message = "Media file updated successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(UpdateMediaFileAsync),
                ["MediaFileId"] = id.ToString()
            });

            return new ApiResponse<MediaFileDto>
            {
                Success = false,
                Message = "Failed to update media file",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<List<MediaFileDto>> GetMediaFilesByCategoryAsync(string category)
    {
        try
        {
            var mediaFiles = await _unitOfWork.MediaFiles.GetByCategoryAsync(category);
            return mediaFiles.Select(MapToMediaFileDto).ToList();
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetMediaFilesByCategoryAsync),
                ["Category"] = category
            });
            throw;
        }
    }

    public async Task<List<MediaFileDto>> GetMediaFilesForArticleAsync(int articleId)
    {
        try
        {
            var mediaFiles = await _unitOfWork.MediaFiles.GetByArticleIdAsync(articleId);
            return mediaFiles.Select(MapToMediaFileDto).ToList();
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetMediaFilesForArticleAsync),
                ["ArticleId"] = articleId.ToString()
            });
            throw;
        }
    }

    public async Task<List<MediaFileDto>> GetMediaFilesForProjectAsync(int projectId)
    {
        try
        {
            var mediaFiles = await _unitOfWork.MediaFiles.GetByProjectIdAsync(projectId);
            return mediaFiles.Select(MapToMediaFileDto).ToList();
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetMediaFilesForProjectAsync),
                ["ProjectId"] = projectId.ToString()
            });
            throw;
        }
    }

    public Task<ApiResponse> ValidateFileAsync(IFormFile file)
    {
        try
        {
            var errors = new List<string>();

            // Check if file is provided
            if (file == null || file.Length == 0)
            {
                errors.Add("No file provided");
            }
            else
            {
                // Check file size
                if (file.Length > _maxFileSize)
                {
                    errors.Add($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB");
                }

                // Check file type
                var allowedTypes = _allowedImageTypes.Concat(_allowedDocumentTypes).ToArray();
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    errors.Add($"File type '{file.ContentType}' is not allowed. Allowed types: {string.Join(", ", allowedTypes)}");
                }

                // Check file extension
                var fileExtension = Path.GetExtension(file.FileName)?.ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx" };
                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    errors.Add($"File extension '{fileExtension}' is not allowed. Allowed extensions: {string.Join(", ", allowedExtensions)}");
                }
            }

            var response = new ApiResponse
            {
                Success = !errors.Any(),
                Message = errors.Any() ? "File validation failed" : "File is valid",
                Errors = errors
            };

            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(ValidateFileAsync)
            });

            return Task.FromResult(new ApiResponse
            {
                Success = false,
                Message = "File validation error",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    public async Task<MediaStorageStatisticsDto> GetStorageStatisticsAsync()
    {
        try
        {
            var allMediaFiles = await _unitOfWork.MediaFiles.GetAllAsync();
            var totalSizeBytes = allMediaFiles.Sum(m => m.FileSize);

            var statistics = new MediaStorageStatisticsDto
            {
                TotalFiles = allMediaFiles.Count(),
                TotalSizeBytes = totalSizeBytes,
                TotalSizeFormatted = FormatFileSize(totalSizeBytes),
                FilesByCategory = allMediaFiles
                    .GroupBy(m => m.Category ?? "Uncategorized")
                    .ToDictionary(g => g.Key, g => g.Count()),
                FilesByType = allMediaFiles
                    .GroupBy(m => m.ContentType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                OrphanedFiles = allMediaFiles.Count(m => m.ArticleId == null && m.ProjectId == null)
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetStorageStatisticsAsync)
            });
            throw;
        }
    }

    public async Task<ApiResponse<int>> CleanupOrphanedFilesAsync(string userId)
    {
        try
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            var orphanedFiles = await _unitOfWork.MediaFiles.GetOrphanedFilesAsync();
            var deletedCount = 0;

            foreach (var mediaFile in orphanedFiles)
            {
                // Delete physical file
                var filePath = Path.Combine("wwwroot", "uploads", mediaFile.FileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                await _unitOfWork.MediaFiles.DeleteAsync(mediaFile.Id);
                deletedCount++;
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _observability.TrackEvent("OrphanedFilesCleanup", new Dictionary<string, string>
            {
                ["DeletedCount"] = deletedCount.ToString(),
                ["CleanedBy"] = userId
            });

            return new ApiResponse<int>
            {
                Success = true,
                Data = deletedCount,
                Message = $"Successfully cleaned up {deletedCount} orphaned files"
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(CleanupOrphanedFilesAsync)
            });

            return new ApiResponse<int>
            {
                Success = false,
                Message = "Failed to cleanup orphaned files",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private static MediaFileDto MapToMediaFileDto(MediaFile mediaFile)
    {
        return new MediaFileDto
        {
            Id = mediaFile.Id,
            FileName = mediaFile.FileName,
            OriginalFileName = mediaFile.OriginalFileName,
            ContentType = mediaFile.ContentType,
            FileSize = mediaFile.FileSize,
            BlobUrl = mediaFile.BlobUrl,
            Category = mediaFile.Category,
            UploadedDate = mediaFile.UploadedDate,
            UploadedBy = mediaFile.UploadedBy,
            ArticleId = mediaFile.ArticleId,
            ProjectId = mediaFile.ProjectId
        };
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
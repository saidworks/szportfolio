using Microsoft.AspNetCore.Components.Forms;
using PortfolioCMS.Frontend.Models;

namespace PortfolioCMS.Frontend.Services;

public class MediaService : IMediaService
{
    private readonly IApiService _apiService;
    private readonly ILogger<MediaService> _logger;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public MediaService(
        IApiService apiService,
        ILogger<MediaService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<string?> UploadImageAsync(IBrowserFile file, string category)
    {
        try
        {
            if (file.Size > MaxFileSize)
            {
                _logger.LogWarning("File {FileName} exceeds maximum size", file.Name);
                return null;
            }

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream(MaxFileSize);
            using var streamContent = new StreamContent(fileStream);
            
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.Name);
            content.Add(new StringContent(category), "category");

            // Note: This would need a custom implementation in ApiService for multipart/form-data
            // For now, returning null as placeholder
            _logger.LogInformation("Uploading file {FileName} to category {Category}", file.Name, category);
            
            return null; // TODO: Implement multipart upload in ApiService
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", file.Name);
            return null;
        }
    }

    public async Task<List<MediaFileDto>?> GetMediaFilesAsync()
    {
        try
        {
            return await _apiService.GetAsync<List<MediaFileDto>>("/api/v1/media");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching media files");
            return null;
        }
    }

    public async Task<bool> DeleteMediaFileAsync(string fileName)
    {
        try
        {
            return await _apiService.DeleteAsync($"/api/v1/media/{Uri.EscapeDataString(fileName)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media file {FileName}", fileName);
            return false;
        }
    }
}

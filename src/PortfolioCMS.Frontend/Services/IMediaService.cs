using Microsoft.AspNetCore.Components.Forms;
using PortfolioCMS.Frontend.Models;

namespace PortfolioCMS.Frontend.Services;

public interface IMediaService
{
    Task<string?> UploadImageAsync(IBrowserFile file, string category);
    Task<List<MediaFileDto>?> GetMediaFilesAsync();
    Task<bool> DeleteMediaFileAsync(string fileName);
}

using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Repositories;

public interface IMediaFileRepository : IRepository<MediaFile>
{
    // MediaFile-specific operations
    Task<IEnumerable<MediaFile>> GetByCategoryAsync(string category);
    Task<IEnumerable<MediaFile>> GetByArticleIdAsync(int articleId);
    Task<IEnumerable<MediaFile>> GetByProjectIdAsync(int projectId);
    Task<IEnumerable<MediaFile>> GetByUserIdAsync(string userId);
    Task<IEnumerable<MediaFile>> GetRecentUploadsAsync(int count);
    Task<MediaFile?> GetByFileNameAsync(string fileName);
    Task<long> GetTotalFileSizeAsync();
    Task<long> GetTotalFileSizeByCategoryAsync(string category);
    Task<long> GetTotalFileSizeByUserAsync(string userId);
    Task<IEnumerable<MediaFile>> GetOrphanedFilesAsync();
    Task<bool> IsFileNameUniqueAsync(string fileName);
}
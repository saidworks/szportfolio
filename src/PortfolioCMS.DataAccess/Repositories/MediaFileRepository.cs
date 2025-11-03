using Microsoft.EntityFrameworkCore;
using PortfolioCMS.DataAccess.Context;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Repositories;

public class MediaFileRepository : BaseRepository<MediaFile>, IMediaFileRepository
{
    public MediaFileRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MediaFile>> GetByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(m => m.Category.ToLower() == category.ToLower())
            .OrderByDescending(m => m.UploadedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MediaFile>> GetByArticleIdAsync(int articleId)
    {
        return await _dbSet
            .Where(m => m.ArticleId == articleId)
            .OrderByDescending(m => m.UploadedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MediaFile>> GetByProjectIdAsync(int projectId)
    {
        return await _dbSet
            .Where(m => m.ProjectId == projectId)
            .OrderByDescending(m => m.UploadedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MediaFile>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(m => m.UploadedBy == userId)
            .OrderByDescending(m => m.UploadedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MediaFile>> GetRecentUploadsAsync(int count)
    {
        return await _dbSet
            .OrderByDescending(m => m.UploadedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<MediaFile?> GetByFileNameAsync(string fileName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.FileName.ToLower() == fileName.ToLower());
    }

    public async Task<long> GetTotalFileSizeAsync()
    {
        return await _dbSet.SumAsync(m => m.FileSize);
    }

    public async Task<long> GetTotalFileSizeByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(m => m.Category.ToLower() == category.ToLower())
            .SumAsync(m => m.FileSize);
    }

    public async Task<long> GetTotalFileSizeByUserAsync(string userId)
    {
        return await _dbSet
            .Where(m => m.UploadedBy == userId)
            .SumAsync(m => m.FileSize);
    }

    public async Task<IEnumerable<MediaFile>> GetOrphanedFilesAsync()
    {
        return await _dbSet
            .Where(m => m.ArticleId == null && m.ProjectId == null)
            .OrderByDescending(m => m.UploadedDate)
            .ToListAsync();
    }

    public async Task<bool> IsFileNameUniqueAsync(string fileName)
    {
        return !await _dbSet
            .AnyAsync(m => m.FileName.ToLower() == fileName.ToLower());
    }
}
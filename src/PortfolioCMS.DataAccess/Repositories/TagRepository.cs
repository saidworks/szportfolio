using Microsoft.EntityFrameworkCore;
using PortfolioCMS.DataAccess.Context;
using PortfolioCMS.DataAccess.Entities;
using System.Text.RegularExpressions;

namespace PortfolioCMS.DataAccess.Repositories;

public class TagRepository : BaseRepository<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Tag?> GetBySlugAsync(string slug)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Slug.ToLower() == slug.ToLower());
    }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<Tag>> GetTagsWithArticleCountAsync()
    {
        return await _dbSet
            .Include(t => t.Articles.Where(a => a.Status == ArticleStatus.Published))
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count)
    {
        return await _dbSet
            .Include(t => t.Articles)
            .Where(t => t.Articles.Any(a => a.Status == ArticleStatus.Published))
            .OrderByDescending(t => t.Articles.Count(a => a.Status == ArticleStatus.Published))
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tag>> GetTagsByArticleIdAsync(int articleId)
    {
        return await _dbSet
            .Include(t => t.Articles)
            .Where(t => t.Articles.Any(a => a.Id == articleId))
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null)
    {
        var query = _dbSet.Where(t => t.Slug.ToLower() == slug.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }
        
        return !await query.AnyAsync();
    }

    public async Task<string> GenerateUniqueSlugAsync(string name)
    {
        var baseSlug = GenerateSlugFromName(name);
        var slug = baseSlug;
        var counter = 1;

        while (!await IsSlugUniqueAsync(slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    public async Task<int> GetArticleCountByTagIdAsync(int tagId)
    {
        var tag = await _dbSet
            .Include(t => t.Articles)
            .FirstOrDefaultAsync(t => t.Id == tagId);

        return tag?.Articles.Count(a => a.Status == ArticleStatus.Published) ?? 0;
    }

    private static string GenerateSlugFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Convert to lowercase
        var slug = name.ToLowerInvariant();

        // Remove invalid characters
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        // Replace spaces and multiple hyphens with single hyphen
        slug = Regex.Replace(slug, @"[\s-]+", "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        // Limit length
        if (slug.Length > 50)
        {
            slug = slug.Substring(0, 50).TrimEnd('-');
        }

        return slug;
    }
}
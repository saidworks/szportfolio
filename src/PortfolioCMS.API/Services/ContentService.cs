using Microsoft.EntityFrameworkCore;
using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.DTOs.Common;
using PortfolioCMS.DataAccess.Entities;
using PortfolioCMS.DataAccess.Repositories;

namespace PortfolioCMS.API.Services;

public class ContentService : IContentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObservabilityService _observability;
    private readonly ILogger<ContentService> _logger;

    public ContentService(
        IUnitOfWork unitOfWork,
        IObservabilityService observability,
        ILogger<ContentService> logger)
    {
        _unitOfWork = unitOfWork;
        _observability = observability;
        _logger = logger;
    }

    public async Task<PagedResult<ArticleDto>> GetPublishedArticlesAsync(ArticleQueryParameters parameters)
    {
        try
        {
            using var operation = _observability.StartOperation("GetPublishedArticles");
            
            var articles = await _unitOfWork.Articles.GetPublishedArticlesAsync();
            
            // Apply search filter
            if (!string.IsNullOrEmpty(parameters.Search))
            {
                articles = articles.Where(a => 
                    a.Title.Contains(parameters.Search, StringComparison.OrdinalIgnoreCase) ||
                    a.Content.Contains(parameters.Search, StringComparison.OrdinalIgnoreCase) ||
                    a.Summary.Contains(parameters.Search, StringComparison.OrdinalIgnoreCase));
            }

            // Apply tag filter
            if (!string.IsNullOrEmpty(parameters.Tag))
            {
                articles = articles.Where(a => a.Tags.Any(t => t.Slug == parameters.Tag));
            }

            // Apply sorting
            articles = parameters.SortBy?.ToLower() switch
            {
                "title" => parameters.SortOrder?.ToLower() == "desc" 
                    ? articles.OrderByDescending(a => a.Title)
                    : articles.OrderBy(a => a.Title),
                "publisheddate" => parameters.SortOrder?.ToLower() == "desc"
                    ? articles.OrderByDescending(a => a.PublishedDate)
                    : articles.OrderBy(a => a.PublishedDate),
                _ => articles.OrderByDescending(a => a.CreatedDate)
            };

            var totalCount = articles.Count();
            var pagedArticles = articles
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();

            var articleDtos = pagedArticles.Select(MapToArticleDto).ToList();

            _observability.TrackMetric("PublishedArticlesRetrieved", articleDtos.Count);

            return new PagedResult<ArticleDto>
            {
                Items = articleDtos,
                TotalCount = totalCount,
                Page = parameters.Page,
                PageSize = parameters.PageSize
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetPublishedArticlesAsync)
            });
            throw;
        }
    }

    public async Task<ArticleDetailDto?> GetArticleByIdAsync(int id)
    {
        try
        {
            using var operation = _observability.StartOperation("GetArticleById", new Dictionary<string, string>
            {
                ["ArticleId"] = id.ToString()
            });

            var article = await _unitOfWork.Articles.GetByIdWithCommentsAsync(id);
            if (article == null || article.Status != ArticleStatus.Published)
            {
                return null;
            }

            var articleDto = MapToArticleDetailDto(article);
            
            _observability.TrackEvent("ArticleViewed", new Dictionary<string, string>
            {
                ["ArticleId"] = id.ToString(),
                ["ArticleTitle"] = article.Title
            });

            return articleDto;
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetArticleByIdAsync),
                ["ArticleId"] = id.ToString()
            });
            throw;
        }
    }

    public async Task<ArticleDetailDto?> GetArticleForEditAsync(int id)
    {
        try
        {
            var article = await _unitOfWork.Articles.GetByIdWithCommentsAsync(id);
            return article != null ? MapToArticleDetailDto(article) : null;
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetArticleForEditAsync),
                ["ArticleId"] = id.ToString()
            });
            throw;
        }
    }

    public async Task<PagedResult<ArticleDto>> SearchArticlesAsync(string query, int page = 1, int pageSize = 10)
    {
        try
        {
            using var operation = _observability.StartOperation("SearchArticles", new Dictionary<string, string>
            {
                ["Query"] = query
            });

            var articles = await _unitOfWork.Articles.SearchArticlesAsync(query);
            var totalCount = articles.Count();
            
            var pagedArticles = articles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var articleDtos = pagedArticles.Select(MapToArticleDto).ToList();

            _observability.TrackEvent("ArticleSearchPerformed", new Dictionary<string, string>
            {
                ["Query"] = query,
                ["ResultCount"] = articleDtos.Count.ToString()
            });

            return new PagedResult<ArticleDto>
            {
                Items = articleDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(SearchArticlesAsync),
                ["Query"] = query
            });
            throw;
        }
    }

    public async Task<PagedResult<ArticleDto>> GetArticlesByTagAsync(string tagSlug, int page = 1, int pageSize = 10)
    {
        try
        {
            var articles = await _unitOfWork.Articles.GetByTagAsync(tagSlug);
            var totalCount = articles.Count();
            
            var pagedArticles = articles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var articleDtos = pagedArticles.Select(MapToArticleDto).ToList();

            return new PagedResult<ArticleDto>
            {
                Items = articleDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetArticlesByTagAsync),
                ["TagSlug"] = tagSlug
            });
            throw;
        }
    }

    public async Task<ApiResponse<ArticleDto>> CreateArticleAsync(CreateArticleDto dto, string userId)
    {
        try
        {
            using var operation = _observability.StartOperation("CreateArticle");

            var article = new Article
            {
                Title = dto.Title,
                Content = dto.Content,
                Summary = dto.Summary,
                Status = dto.Status,
                FeaturedImageUrl = dto.FeaturedImageUrl,
                MetaDescription = dto.MetaDescription,
                MetaKeywords = dto.MetaKeywords,
                UserId = userId,
                CreatedDate = DateTime.UtcNow,
                PublishedDate = dto.Status == ArticleStatus.Published ? DateTime.UtcNow : null
            };

            // Handle tags
            if (dto.TagNames.Any())
            {
                var tags = new List<Tag>();
                foreach (var tagName in dto.TagNames)
                {
                    var existingTag = await _unitOfWork.Tags.GetByNameAsync(tagName);
                    if (existingTag != null)
                    {
                        tags.Add(existingTag);
                    }
                    else
                    {
                        var newTag = new Tag
                        {
                            Name = tagName,
                            Slug = GenerateSlug(tagName),
                            CreatedDate = DateTime.UtcNow
                        };
                        tags.Add(await _unitOfWork.Tags.AddAsync(newTag));
                    }
                }
                article.Tags = tags;
            }

            var createdArticle = await _unitOfWork.Articles.AddAsync(article);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("ArticleCreated", new Dictionary<string, string>
            {
                ["ArticleId"] = createdArticle.Id.ToString(),
                ["ArticleTitle"] = createdArticle.Title,
                ["Status"] = createdArticle.Status.ToString()
            });

            return new ApiResponse<ArticleDto>
            {
                Success = true,
                Data = MapToArticleDto(createdArticle),
                Message = "Article created successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(CreateArticleAsync)
            });

            return new ApiResponse<ArticleDto>
            {
                Success = false,
                Message = "Failed to create article",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<ArticleDto>> UpdateArticleAsync(int id, UpdateArticleDto dto, string userId)
    {
        try
        {
            var article = await _unitOfWork.Articles.GetByIdAsync(id);
            if (article == null)
            {
                return new ApiResponse<ArticleDto>
                {
                    Success = false,
                    Message = "Article not found"
                };
            }

            article.Title = dto.Title;
            article.Content = dto.Content;
            article.Summary = dto.Summary;
            article.Status = dto.Status;
            article.FeaturedImageUrl = dto.FeaturedImageUrl;
            article.MetaDescription = dto.MetaDescription;
            article.MetaKeywords = dto.MetaKeywords;

            if (dto.Status == ArticleStatus.Published && article.PublishedDate == null)
            {
                article.PublishedDate = DateTime.UtcNow;
            }

            // Handle tags
            article.Tags.Clear();
            if (dto.TagNames.Any())
            {
                var tags = new List<Tag>();
                foreach (var tagName in dto.TagNames)
                {
                    var existingTag = await _unitOfWork.Tags.GetByNameAsync(tagName);
                    if (existingTag != null)
                    {
                        tags.Add(existingTag);
                    }
                    else
                    {
                        var newTag = new Tag
                        {
                            Name = tagName,
                            Slug = GenerateSlug(tagName),
                            CreatedDate = DateTime.UtcNow
                        };
                        tags.Add(await _unitOfWork.Tags.AddAsync(newTag));
                    }
                }
                article.Tags = tags;
            }

            await _unitOfWork.Articles.UpdateAsync(article);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("ArticleUpdated", new Dictionary<string, string>
            {
                ["ArticleId"] = id.ToString(),
                ["ArticleTitle"] = article.Title
            });

            return new ApiResponse<ArticleDto>
            {
                Success = true,
                Data = MapToArticleDto(article),
                Message = "Article updated successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(UpdateArticleAsync),
                ["ArticleId"] = id.ToString()
            });

            return new ApiResponse<ArticleDto>
            {
                Success = false,
                Message = "Failed to update article",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> DeleteArticleAsync(int id, string userId)
    {
        try
        {
            var article = await _unitOfWork.Articles.GetByIdAsync(id);
            if (article == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Article not found"
                };
            }

            await _unitOfWork.Articles.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("ArticleDeleted", new Dictionary<string, string>
            {
                ["ArticleId"] = id.ToString(),
                ["ArticleTitle"] = article.Title
            });

            return new ApiResponse
            {
                Success = true,
                Message = "Article deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(DeleteArticleAsync),
                ["ArticleId"] = id.ToString()
            });

            return new ApiResponse
            {
                Success = false,
                Message = "Failed to delete article",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<ArticleDto>> PublishArticleAsync(int id, string userId)
    {
        try
        {
            var article = await _unitOfWork.Articles.GetByIdAsync(id);
            if (article == null)
            {
                return new ApiResponse<ArticleDto>
                {
                    Success = false,
                    Message = "Article not found"
                };
            }

            article.Status = ArticleStatus.Published;
            article.PublishedDate = DateTime.UtcNow;

            await _unitOfWork.Articles.UpdateAsync(article);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("ArticlePublished", new Dictionary<string, string>
            {
                ["ArticleId"] = id.ToString(),
                ["ArticleTitle"] = article.Title
            });

            return new ApiResponse<ArticleDto>
            {
                Success = true,
                Data = MapToArticleDto(article),
                Message = "Article published successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(PublishArticleAsync),
                ["ArticleId"] = id.ToString()
            });

            return new ApiResponse<ArticleDto>
            {
                Success = false,
                Message = "Failed to publish article",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<ArticleDto>> UnpublishArticleAsync(int id, string userId)
    {
        try
        {
            var article = await _unitOfWork.Articles.GetByIdAsync(id);
            if (article == null)
            {
                return new ApiResponse<ArticleDto>
                {
                    Success = false,
                    Message = "Article not found"
                };
            }

            article.Status = ArticleStatus.Draft;

            await _unitOfWork.Articles.UpdateAsync(article);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("ArticleUnpublished", new Dictionary<string, string>
            {
                ["ArticleId"] = id.ToString(),
                ["ArticleTitle"] = article.Title
            });

            return new ApiResponse<ArticleDto>
            {
                Success = true,
                Data = MapToArticleDto(article),
                Message = "Article unpublished successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(UnpublishArticleAsync),
                ["ArticleId"] = id.ToString()
            });

            return new ApiResponse<ArticleDto>
            {
                Success = false,
                Message = "Failed to unpublish article",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<PagedResult<ArticleDto>> GetAllArticlesAsync(ArticleQueryParameters parameters)
    {
        try
        {
            var articles = await _unitOfWork.Articles.GetAllAsync();
            
            // Apply filters
            if (parameters.Status.HasValue)
            {
                articles = articles.Where(a => a.Status == parameters.Status.Value);
            }

            if (!string.IsNullOrEmpty(parameters.Search))
            {
                articles = articles.Where(a => 
                    a.Title.Contains(parameters.Search, StringComparison.OrdinalIgnoreCase) ||
                    a.Content.Contains(parameters.Search, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            articles = parameters.SortBy?.ToLower() switch
            {
                "title" => parameters.SortOrder?.ToLower() == "desc" 
                    ? articles.OrderByDescending(a => a.Title)
                    : articles.OrderBy(a => a.Title),
                "status" => parameters.SortOrder?.ToLower() == "desc"
                    ? articles.OrderByDescending(a => a.Status)
                    : articles.OrderBy(a => a.Status),
                _ => articles.OrderByDescending(a => a.CreatedDate)
            };

            var totalCount = articles.Count();
            var pagedArticles = articles
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();

            return new PagedResult<ArticleDto>
            {
                Items = pagedArticles.Select(MapToArticleDto).ToList(),
                TotalCount = totalCount,
                Page = parameters.Page,
                PageSize = parameters.PageSize
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetAllArticlesAsync)
            });
            throw;
        }
    }

    // Project methods implementation continues...
    public async Task<List<ProjectDto>> GetAllProjectsAsync()
    {
        try
        {
            var projects = await _unitOfWork.Projects.GetAllAsync();
            return projects.Select(MapToProjectDto).ToList();
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetAllProjectsAsync)
            });
            throw;
        }
    }

    public async Task<List<ProjectDto>> GetActiveProjectsAsync()
    {
        try
        {
            var projects = await _unitOfWork.Projects.GetActiveProjectsAsync();
            return projects.Select(MapToProjectDto).ToList();
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetActiveProjectsAsync)
            });
            throw;
        }
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        try
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            return project != null ? MapToProjectDto(project) : null;
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetProjectByIdAsync),
                ["ProjectId"] = id.ToString()
            });
            throw;
        }
    }

    public async Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectDto dto, string userId)
    {
        try
        {
            var project = new Project
            {
                Title = dto.Title,
                Description = dto.Description,
                TechnologyStack = dto.TechnologyStack,
                ProjectUrl = dto.ProjectUrl,
                GitHubUrl = dto.GitHubUrl,
                ImageUrl = dto.ImageUrl,
                CompletedDate = dto.CompletedDate,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive
            };

            var createdProject = await _unitOfWork.Projects.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("ProjectCreated", new Dictionary<string, string>
            {
                ["ProjectId"] = createdProject.Id.ToString(),
                ["ProjectTitle"] = createdProject.Title
            });

            return new ApiResponse<ProjectDto>
            {
                Success = true,
                Data = MapToProjectDto(createdProject),
                Message = "Project created successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(CreateProjectAsync)
            });

            return new ApiResponse<ProjectDto>
            {
                Success = false,
                Message = "Failed to create project",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<ProjectDto>> UpdateProjectAsync(int id, UpdateProjectDto dto, string userId)
    {
        try
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null)
            {
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "Project not found"
                };
            }

            project.Title = dto.Title;
            project.Description = dto.Description;
            project.TechnologyStack = dto.TechnologyStack;
            project.ProjectUrl = dto.ProjectUrl;
            project.GitHubUrl = dto.GitHubUrl;
            project.ImageUrl = dto.ImageUrl;
            project.CompletedDate = dto.CompletedDate;
            project.DisplayOrder = dto.DisplayOrder;
            project.IsActive = dto.IsActive;

            await _unitOfWork.Projects.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("ProjectUpdated", new Dictionary<string, string>
            {
                ["ProjectId"] = id.ToString(),
                ["ProjectTitle"] = project.Title
            });

            return new ApiResponse<ProjectDto>
            {
                Success = true,
                Data = MapToProjectDto(project),
                Message = "Project updated successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(UpdateProjectAsync),
                ["ProjectId"] = id.ToString()
            });

            return new ApiResponse<ProjectDto>
            {
                Success = false,
                Message = "Failed to update project",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> DeleteProjectAsync(int id, string userId)
    {
        try
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Project not found"
                };
            }

            await _unitOfWork.Projects.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("ProjectDeleted", new Dictionary<string, string>
            {
                ["ProjectId"] = id.ToString(),
                ["ProjectTitle"] = project.Title
            });

            return new ApiResponse
            {
                Success = true,
                Message = "Project deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(DeleteProjectAsync),
                ["ProjectId"] = id.ToString()
            });

            return new ApiResponse
            {
                Success = false,
                Message = "Failed to delete project",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<List<TagDto>> GetAllTagsAsync()
    {
        try
        {
            var tags = await _unitOfWork.Tags.GetAllAsync();
            return tags.Select(MapToTagDto).ToList();
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetAllTagsAsync)
            });
            throw;
        }
    }

    public async Task<List<TagDto>> GetPopularTagsAsync(int count = 10)
    {
        try
        {
            var tags = await _unitOfWork.Tags.GetPopularTagsAsync(count);
            return tags.Select(MapToTagDto).ToList();
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(GetPopularTagsAsync)
            });
            throw;
        }
    }

    public async Task<ApiResponse<TagDto>> CreateTagAsync(CreateTagDto dto, string userId)
    {
        try
        {
            var existingTag = await _unitOfWork.Tags.GetByNameAsync(dto.Name);
            if (existingTag != null)
            {
                return new ApiResponse<TagDto>
                {
                    Success = false,
                    Message = "Tag with this name already exists"
                };
            }

            var tag = new Tag
            {
                Name = dto.Name,
                Slug = GenerateSlug(dto.Name),
                Description = dto.Description,
                CreatedDate = DateTime.UtcNow
            };

            var createdTag = await _unitOfWork.Tags.AddAsync(tag);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("TagCreated", new Dictionary<string, string>
            {
                ["TagId"] = createdTag.Id.ToString(),
                ["TagName"] = createdTag.Name
            });

            return new ApiResponse<TagDto>
            {
                Success = true,
                Data = MapToTagDto(createdTag),
                Message = "Tag created successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(CreateTagAsync)
            });

            return new ApiResponse<TagDto>
            {
                Success = false,
                Message = "Failed to create tag",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<TagDto>> UpdateTagAsync(int id, UpdateTagDto dto, string userId)
    {
        try
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            if (tag == null)
            {
                return new ApiResponse<TagDto>
                {
                    Success = false,
                    Message = "Tag not found"
                };
            }

            tag.Name = dto.Name;
            tag.Slug = GenerateSlug(dto.Name);
            tag.Description = dto.Description;

            await _unitOfWork.Tags.UpdateAsync(tag);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("TagUpdated", new Dictionary<string, string>
            {
                ["TagId"] = id.ToString(),
                ["TagName"] = tag.Name
            });

            return new ApiResponse<TagDto>
            {
                Success = true,
                Data = MapToTagDto(tag),
                Message = "Tag updated successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(UpdateTagAsync),
                ["TagId"] = id.ToString()
            });

            return new ApiResponse<TagDto>
            {
                Success = false,
                Message = "Failed to update tag",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse> DeleteTagAsync(int id, string userId)
    {
        try
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            if (tag == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Tag not found"
                };
            }

            await _unitOfWork.Tags.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _observability.TrackEvent("TagDeleted", new Dictionary<string, string>
            {
                ["TagId"] = id.ToString(),
                ["TagName"] = tag.Name
            });

            return new ApiResponse
            {
                Success = true,
                Message = "Tag deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _observability.TrackException(ex, new Dictionary<string, string>
            {
                ["Method"] = nameof(DeleteTagAsync),
                ["TagId"] = id.ToString()
            });

            return new ApiResponse
            {
                Success = false,
                Message = "Failed to delete tag",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    // Helper methods for mapping entities to DTOs
    private static ArticleDto MapToArticleDto(Article article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Summary = article.Summary,
            CreatedDate = article.CreatedDate,
            PublishedDate = article.PublishedDate,
            Status = article.Status,
            FeaturedImageUrl = article.FeaturedImageUrl,
            MetaDescription = article.MetaDescription,
            MetaKeywords = article.MetaKeywords,
            Tags = article.Tags?.Select(MapToTagDto).ToList() ?? new List<TagDto>(),
            CommentCount = article.Comments?.Count(c => c.Status == CommentStatus.Approved) ?? 0
        };
    }

    private static ArticleDetailDto MapToArticleDetailDto(Article article)
    {
        return new ArticleDetailDto
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            Summary = article.Summary,
            CreatedDate = article.CreatedDate,
            PublishedDate = article.PublishedDate,
            Status = article.Status,
            FeaturedImageUrl = article.FeaturedImageUrl,
            MetaDescription = article.MetaDescription,
            MetaKeywords = article.MetaKeywords,
            Tags = article.Tags?.Select(MapToTagDto).ToList() ?? new List<TagDto>(),
            Comments = article.Comments?.Where(c => c.Status == CommentStatus.Approved)
                .Select(MapToCommentDto).ToList() ?? new List<CommentDto>(),
            CommentCount = article.Comments?.Count(c => c.Status == CommentStatus.Approved) ?? 0
        };
    }

    private static ProjectDto MapToProjectDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            TechnologyStack = project.TechnologyStack,
            ProjectUrl = project.ProjectUrl,
            GitHubUrl = project.GitHubUrl,
            ImageUrl = project.ImageUrl,
            CompletedDate = project.CompletedDate,
            DisplayOrder = project.DisplayOrder,
            IsActive = project.IsActive,
            MediaFiles = project.MediaFiles?.Select(MapToMediaFileDto).ToList() ?? new List<MediaFileDto>()
        };
    }

    private static TagDto MapToTagDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Slug = tag.Slug,
            Description = tag.Description,
            CreatedDate = tag.CreatedDate,
            ArticleCount = tag.Articles?.Count ?? 0
        };
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

    private static string GenerateSlug(string input)
    {
        return input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("'", "")
            .Replace("\"", "");
    }
}
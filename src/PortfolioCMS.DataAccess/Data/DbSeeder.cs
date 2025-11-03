using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PortfolioCMS.DataAccess.Context;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.DataAccess.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AspNetUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");

            // Seed roles first
            await SeedRolesAsync(roleManager, logger);

            // Seed admin user
            var adminUser = await SeedAdminUserAsync(userManager, logger);

            // Seed sample data with admin user context
            await SeedSampleDataAsync(context, adminUser?.Id, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        var roles = new[] { "Admin", "Viewer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (result.Succeeded)
                {
                    logger.LogInformation("Created role: {Role}", role);
                }
                else
                {
                    logger.LogError("Failed to create role: {Role}. Errors: {Errors}", 
                        role, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private static async Task<AspNetUser?> SeedAdminUserAsync(UserManager<AspNetUser> userManager, ILogger logger)
    {
        const string adminEmail = "admin@portfoliocms.com";
        const string adminPassword = "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AspNetUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Portfolio",
                LastName = "Admin",
                CreatedDate = DateTime.UtcNow,
                Role = "Admin"
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Created admin user: {Email}", adminEmail);
                return adminUser;
            }
            else
            {
                logger.LogError("Failed to create admin user: {Email}. Errors: {Errors}", 
                    adminEmail, string.Join(", ", result.Errors.Select(e => e.Description)));
                return null;
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists: {Email}", adminEmail);
            return adminUser;
        }
    }

    private static async Task SeedSampleDataAsync(ApplicationDbContext context, string? adminUserId, ILogger logger)
    {
        // Seed tags
        await SeedTagsAsync(context, logger);

        // Seed projects
        await SeedProjectsAsync(context, logger);

        // Seed articles with admin user if available
        if (!string.IsNullOrEmpty(adminUserId))
        {
            await SeedArticlesAsync(context, adminUserId, logger);
        }

        // Seed sample comments
        await SeedSampleCommentsAsync(context, logger);

        await context.SaveChangesAsync();
        logger.LogInformation("Sample data seeded successfully");
    }

    private static async Task SeedTagsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Tags.AnyAsync())
        {
            return; // Tags already exist
        }

        var tags = new[]
        {
            new Tag { Name = "C#", Slug = "csharp", Description = "C# programming language", CreatedDate = DateTime.UtcNow },
            new Tag { Name = "ASP.NET Core", Slug = "aspnet-core", Description = "ASP.NET Core framework", CreatedDate = DateTime.UtcNow },
            new Tag { Name = "Entity Framework", Slug = "entity-framework", Description = "Entity Framework ORM", CreatedDate = DateTime.UtcNow },
            new Tag { Name = "Blazor", Slug = "blazor", Description = "Blazor web framework", CreatedDate = DateTime.UtcNow },
            new Tag { Name = "Azure", Slug = "azure", Description = "Microsoft Azure cloud platform", CreatedDate = DateTime.UtcNow },
            new Tag { Name = "SQL Server", Slug = "sql-server", Description = "Microsoft SQL Server database", CreatedDate = DateTime.UtcNow },
            new Tag { Name = "JavaScript", Slug = "javascript", Description = "JavaScript programming language", CreatedDate = DateTime.UtcNow },
            new Tag { Name = "React", Slug = "react", Description = "React JavaScript library", CreatedDate = DateTime.UtcNow },
            new Tag { Name = "TypeScript", Slug = "typescript", Description = "TypeScript programming language", CreatedDate = DateTime.UtcNow },
            new Tag { Name = "Web Development", Slug = "web-development", Description = "Web development technologies", CreatedDate = DateTime.UtcNow }
        };

        context.Tags.AddRange(tags);
        logger.LogInformation("Seeded {Count} tags", tags.Length);
    }

    private static async Task SeedProjectsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Projects.AnyAsync())
        {
            return; // Projects already exist
        }

        var projects = new[]
        {
            new Project
            {
                Title = "Portfolio CMS",
                Description = "A comprehensive content management system built with ASP.NET Core, Blazor, and Azure SQL Database. Features include article management, comment moderation, media uploads, and admin dashboard.",
                TechnologyStack = "ASP.NET Core, Blazor Server, Entity Framework Core, Azure SQL Database, Azure App Service",
                GitHubUrl = "https://github.com/example/portfolio-cms",
                CompletedDate = DateTime.UtcNow.AddDays(-30),
                DisplayOrder = 1,
                IsActive = true
            },
            new Project
            {
                Title = "E-Commerce Platform",
                Description = "A scalable e-commerce platform with product catalog, shopping cart, payment processing, and order management. Built with microservices architecture.",
                TechnologyStack = "ASP.NET Core Web API, React, TypeScript, PostgreSQL, Docker, Kubernetes",
                ProjectUrl = "https://demo-ecommerce.example.com",
                GitHubUrl = "https://github.com/example/ecommerce-platform",
                CompletedDate = DateTime.UtcNow.AddDays(-60),
                DisplayOrder = 2,
                IsActive = true
            },
            new Project
            {
                Title = "Task Management System",
                Description = "A collaborative task management application with real-time updates, team collaboration features, and project tracking capabilities.",
                TechnologyStack = "ASP.NET Core, SignalR, Angular, SQL Server, Azure DevOps",
                ProjectUrl = "https://taskmanager.example.com",
                CompletedDate = DateTime.UtcNow.AddDays(-90),
                DisplayOrder = 3,
                IsActive = true
            }
        };

        context.Projects.AddRange(projects);
        logger.LogInformation("Seeded {Count} projects", projects.Length);
    }

    private static async Task SeedArticlesAsync(ApplicationDbContext context, string adminUserId, ILogger logger)
    {
        if (await context.Articles.AnyAsync())
        {
            return; // Articles already exist
        }

        var csharpTag = await context.Tags.FirstOrDefaultAsync(t => t.Slug == "csharp");
        var aspnetTag = await context.Tags.FirstOrDefaultAsync(t => t.Slug == "aspnet-core");
        var blazorTag = await context.Tags.FirstOrDefaultAsync(t => t.Slug == "blazor");
        var azureTag = await context.Tags.FirstOrDefaultAsync(t => t.Slug == "azure");

        var articles = new[]
        {
            new Article
            {
                Title = "Getting Started with ASP.NET Core and Blazor",
                Summary = "Learn how to build modern web applications using ASP.NET Core and Blazor Server. This comprehensive guide covers project setup, component creation, and best practices.",
                Content = @"<h2>Introduction</h2>
<p>ASP.NET Core and Blazor represent the future of web development in the .NET ecosystem. In this article, we'll explore how to get started with these powerful technologies.</p>

<h2>Setting Up Your Development Environment</h2>
<p>Before we begin, make sure you have the following installed:</p>
<ul>
<li>.NET 8 SDK</li>
<li>Visual Studio 2022 or Visual Studio Code</li>
<li>SQL Server LocalDB (for data storage)</li>
</ul>

<h2>Creating Your First Blazor Server Project</h2>
<p>Let's start by creating a new Blazor Server project using the .NET CLI:</p>
<pre><code>dotnet new blazorserver -n MyBlazorApp
cd MyBlazorApp
dotnet run</code></pre>

<h2>Understanding Blazor Components</h2>
<p>Blazor components are the building blocks of your application. They combine HTML markup with C# code to create reusable UI elements.</p>

<h2>Conclusion</h2>
<p>ASP.NET Core and Blazor provide a powerful platform for building modern web applications. With server-side rendering and the ability to write C# instead of JavaScript, developers can be more productive and maintainable.</p>",
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                PublishedDate = DateTime.UtcNow.AddDays(-15),
                Status = ArticleStatus.Published,
                UserId = adminUserId,
                MetaDescription = "Learn how to build modern web applications using ASP.NET Core and Blazor Server",
                MetaKeywords = "ASP.NET Core, Blazor, C#, Web Development"
            },
            new Article
            {
                Title = "Deploying .NET Applications to Azure",
                Summary = "A step-by-step guide to deploying your .NET applications to Microsoft Azure. Covers App Service, SQL Database, and best practices for production deployments.",
                Content = @"<h2>Introduction</h2>
<p>Microsoft Azure provides excellent hosting options for .NET applications. In this guide, we'll walk through the process of deploying a complete application to Azure.</p>

<h2>Azure App Service</h2>
<p>Azure App Service is a fully managed platform for building, deploying, and scaling web apps. It supports multiple programming languages including .NET.</p>

<h2>Setting Up Azure SQL Database</h2>
<p>For data storage, we'll use Azure SQL Database, which provides a fully managed SQL database service with built-in intelligence.</p>

<h2>Deployment Strategies</h2>
<p>There are several ways to deploy your application to Azure:</p>
<ul>
<li>Visual Studio publish</li>
<li>Azure DevOps pipelines</li>
<li>GitHub Actions</li>
<li>Azure CLI</li>
</ul>

<h2>Best Practices</h2>
<p>When deploying to production, consider these best practices:</p>
<ul>
<li>Use Application Insights for monitoring</li>
<li>Implement proper logging</li>
<li>Set up automated backups</li>
<li>Configure SSL certificates</li>
</ul>",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                PublishedDate = DateTime.UtcNow.AddDays(-10),
                Status = ArticleStatus.Published,
                UserId = adminUserId,
                MetaDescription = "Step-by-step guide to deploying .NET applications to Microsoft Azure",
                MetaKeywords = "Azure, .NET, Deployment, App Service, SQL Database"
            },
            new Article
            {
                Title = "Building Responsive Web Components with Blazor",
                Summary = "Learn how to create responsive and interactive web components using Blazor. Covers component lifecycle, data binding, and event handling.",
                Content = @"<h2>Introduction</h2>
<p>Blazor components are powerful building blocks that allow you to create rich, interactive web applications using C# instead of JavaScript.</p>

<h2>Component Lifecycle</h2>
<p>Understanding the Blazor component lifecycle is crucial for building efficient applications. The main lifecycle methods include:</p>
<ul>
<li>OnInitialized / OnInitializedAsync</li>
<li>OnParametersSet / OnParametersSetAsync</li>
<li>OnAfterRender / OnAfterRenderAsync</li>
</ul>

<h2>Data Binding</h2>
<p>Blazor provides powerful data binding capabilities that make it easy to keep your UI in sync with your data.</p>

<h2>Event Handling</h2>
<p>Handling user interactions in Blazor is straightforward with built-in event handlers and custom event callbacks.</p>

<h2>Best Practices</h2>
<p>Follow these best practices when building Blazor components:</p>
<ul>
<li>Keep components small and focused</li>
<li>Use parameters for component communication</li>
<li>Implement proper disposal for resources</li>
<li>Optimize rendering performance</li>
</ul>",
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                Status = ArticleStatus.Draft,
                UserId = adminUserId,
                MetaDescription = "Learn how to create responsive web components using Blazor",
                MetaKeywords = "Blazor, Components, Web Development, C#"
            }
        };

        context.Articles.AddRange(articles);
        await context.SaveChangesAsync();

        // Add tags to articles
        if (csharpTag != null && aspnetTag != null && blazorTag != null && azureTag != null)
        {
            var article1 = articles[0];
            var article2 = articles[1];
            var article3 = articles[2];

            // Article 1 tags
            article1.Tags.Add(csharpTag);
            article1.Tags.Add(aspnetTag);
            article1.Tags.Add(blazorTag);

            // Article 2 tags
            article2.Tags.Add(csharpTag);
            article2.Tags.Add(aspnetTag);
            article2.Tags.Add(azureTag);

            // Article 3 tags
            article3.Tags.Add(csharpTag);
            article3.Tags.Add(blazorTag);
        }

        logger.LogInformation("Seeded {Count} articles", articles.Length);
    }

    private static async Task SeedSampleCommentsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Comments.AnyAsync())
        {
            return; // Comments already exist
        }

        // Get published articles to add comments to
        var publishedArticles = await context.Articles
            .Where(a => a.Status == ArticleStatus.Published)
            .ToListAsync();

        if (!publishedArticles.Any())
        {
            logger.LogInformation("No published articles found, skipping comment seeding");
            return;
        }

        var comments = new List<Comment>();

        foreach (var article in publishedArticles.Take(2)) // Add comments to first 2 articles
        {
            comments.AddRange(new[]
            {
                new Comment
                {
                    AuthorName = "John Developer",
                    AuthorEmail = "john.dev@example.com",
                    Content = "Great article! This really helped me understand the concepts better. Looking forward to more content like this.",
                    SubmittedDate = DateTime.UtcNow.AddDays(-5),
                    Status = CommentStatus.Approved,
                    ArticleId = article.Id,
                    IpAddress = "192.168.1.100",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
                },
                new Comment
                {
                    AuthorName = "Sarah Tech",
                    AuthorEmail = "sarah.tech@example.com",
                    Content = "Thanks for sharing this! I've been struggling with this topic and your explanation made it much clearer.",
                    SubmittedDate = DateTime.UtcNow.AddDays(-3),
                    Status = CommentStatus.Approved,
                    ArticleId = article.Id,
                    IpAddress = "10.0.0.50",
                    UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36"
                },
                new Comment
                {
                    AuthorName = "Mike Student",
                    AuthorEmail = "mike.student@example.com",
                    Content = "This is exactly what I was looking for! Could you write a follow-up article about advanced scenarios?",
                    SubmittedDate = DateTime.UtcNow.AddDays(-1),
                    Status = CommentStatus.Pending,
                    ArticleId = article.Id,
                    IpAddress = "172.16.0.25",
                    UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36"
                }
            });
        }

        context.Comments.AddRange(comments);
        logger.LogInformation("Seeded {Count} sample comments", comments.Count);
    }
}
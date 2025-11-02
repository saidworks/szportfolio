using Microsoft.AspNetCore.Identity;
using PortfolioCMS.Models.Entities;

namespace PortfolioCMS.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed roles
        await SeedRolesAsync(roleManager);

        // Seed admin user
        await SeedAdminUserAsync(userManager);

        // Seed sample data
        await SeedSampleDataAsync(context, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Viewer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<User> userManager)
    {
        const string adminEmail = "admin@portfoliocms.com";
        const string adminPassword = "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                CreatedDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    private static async Task SeedSampleDataAsync(ApplicationDbContext context, UserManager<User> userManager)
    {
        // Get admin user
        var adminUser = await userManager.FindByEmailAsync("admin@portfoliocms.com");
        if (adminUser == null) return;

        // Seed tags if they don't exist
        if (!context.Tags.Any())
        {
            var tags = new[]
            {
                new Tag { Name = "C#", Slug = "csharp" },
                new Tag { Name = "ASP.NET Core", Slug = "aspnet-core" },
                new Tag { Name = "Blazor", Slug = "blazor" },
                new Tag { Name = "Entity Framework", Slug = "entity-framework" },
                new Tag { Name = "MySQL", Slug = "mysql" },
                new Tag { Name = "Web Development", Slug = "web-development" }
            };

            context.Tags.AddRange(tags);
            await context.SaveChangesAsync();
        }

        // Seed sample article if none exist
        if (!context.Articles.Any())
        {
            var tags = context.Tags.Take(3).ToList();
            var sampleArticle = new Article
            {
                Title = "Welcome to Portfolio CMS",
                Content = "<p>This is a sample article to demonstrate the Portfolio CMS functionality. This system allows you to manage your portfolio content, including articles, projects, and personal information.</p><p>Key features include:</p><ul><li>Rich text editing</li><li>Comment moderation</li><li>Media management</li><li>Responsive design</li></ul>",
                Summary = "A sample article demonstrating the Portfolio CMS features and capabilities.",
                CreatedDate = DateTime.UtcNow,
                PublishedDate = DateTime.UtcNow,
                Status = ArticleStatus.Published,
                UserId = adminUser.Id,
                Tags = tags
            };

            context.Articles.Add(sampleArticle);
            await context.SaveChangesAsync();
        }

        // Seed sample project if none exist
        if (!context.Projects.Any())
        {
            var sampleProject = new Project
            {
                Title = "Portfolio CMS",
                Description = "A full-stack content management system built with ASP.NET Core and Blazor for managing personal portfolios and blogs.",
                TechnologyStack = "ASP.NET Core, Blazor Server, Entity Framework Core, MySQL, Bootstrap",
                GitHubUrl = "https://github.com/example/portfolio-cms",
                CompletedDate = DateTime.UtcNow,
                DisplayOrder = 1
            };

            context.Projects.Add(sampleProject);
            await context.SaveChangesAsync();
        }

        // Seed sample education if none exist
        if (!context.Education.Any())
        {
            var sampleEducation = new Education
            {
                Institution = "University of Technology",
                Degree = "Bachelor of Science",
                FieldOfStudy = "Computer Science",
                StartDate = new DateTime(2018, 9, 1),
                EndDate = new DateTime(2022, 6, 30),
                Description = "Focused on software engineering, web development, and database systems.",
                DisplayOrder = 1
            };

            context.Education.Add(sampleEducation);
            await context.SaveChangesAsync();
        }

        // Seed sample experience if none exist
        if (!context.Experience.Any())
        {
            var sampleExperience = new Experience
            {
                Company = "Tech Solutions Inc.",
                Position = "Software Developer",
                Location = "Remote",
                StartDate = new DateTime(2022, 7, 1),
                EndDate = null,
                IsCurrent = true,
                Description = "Developing web applications using ASP.NET Core, implementing RESTful APIs, and working with modern frontend frameworks.",
                DisplayOrder = 1
            };

            context.Experience.Add(sampleExperience);
            await context.SaveChangesAsync();
        }

        // Seed sample languages if none exist
        if (!context.Languages.Any())
        {
            var languages = new[]
            {
                new Language { Name = "English", ProficiencyLevel = "Native", DisplayOrder = 1 },
                new Language { Name = "Spanish", ProficiencyLevel = "Intermediate", DisplayOrder = 2 }
            };

            context.Languages.AddRange(languages);
            await context.SaveChangesAsync();
        }

        // Seed sample hobbies if none exist
        if (!context.Hobbies.Any())
        {
            var hobbies = new[]
            {
                new Hobby { Name = "Programming", Description = "Building personal projects and contributing to open source", DisplayOrder = 1 },
                new Hobby { Name = "Photography", Description = "Landscape and street photography", DisplayOrder = 2 }
            };

            context.Hobbies.AddRange(hobbies);
            await context.SaveChangesAsync();
        }
    }
}
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortfolioCMS.Models.Entities;

namespace PortfolioCMS.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<Article> Articles { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<MediaFile> MediaFiles { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }
    public DbSet<Education> Education { get; set; }
    public DbSet<Experience> Experience { get; set; }
    public DbSet<Certification> Certifications { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Hobby> Hobbies { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Configure Article-Tag many-to-many relationship
        builder.Entity<Article>()
            .HasMany(a => a.Tags)
            .WithMany(t => t.Articles)
            .UsingEntity(j => j.ToTable("ArticleTags"));

        // Configure Article-Comment one-to-many relationship
        builder.Entity<Comment>()
            .HasOne(c => c.Article)
            .WithMany(a => a.Comments)
            .HasForeignKey(c => c.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure User-Article one-to-many relationship
        builder.Entity<Article>()
            .HasOne(a => a.User)
            .WithMany(u => u.Articles)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure indexes for better performance
        builder.Entity<Article>()
            .HasIndex(a => a.Status);
        
        builder.Entity<Article>()
            .HasIndex(a => a.PublishedDate);
        
        builder.Entity<Tag>()
            .HasIndex(t => t.Slug)
            .IsUnique();
        
        builder.Entity<Comment>()
            .HasIndex(c => c.Status);
        
        builder.Entity<Project>()
            .HasIndex(p => p.DisplayOrder);
        
        builder.Entity<Education>()
            .HasIndex(e => e.DisplayOrder);
        
        builder.Entity<Experience>()
            .HasIndex(e => e.DisplayOrder);
        
        builder.Entity<Certification>()
            .HasIndex(c => c.DisplayOrder);
        
        builder.Entity<Language>()
            .HasIndex(l => l.DisplayOrder);
        
        builder.Entity<Hobby>()
            .HasIndex(h => h.DisplayOrder);
    }
}
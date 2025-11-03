using Microsoft.EntityFrameworkCore.Storage;

namespace PortfolioCMS.DataAccess.Repositories;

public interface IUnitOfWork : IDisposable
{
    // Repository properties
    IArticleRepository Articles { get; }
    ICommentRepository Comments { get; }
    ITagRepository Tags { get; }
    IProjectRepository Projects { get; }
    IMediaFileRepository MediaFiles { get; }

    // Transaction management
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync();
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync();
    Task RollbackTransactionAsync(CancellationToken cancellationToken);

    // Bulk operations
    Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
    Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken, params object[] parameters);

    // Database operations
    Task<bool> CanConnectAsync();
    Task EnsureCreatedAsync();
    Task EnsureDeletedAsync();
}
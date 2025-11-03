using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PortfolioCMS.DataAccess.Context;

namespace PortfolioCMS.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    // Lazy-loaded repositories
    private IArticleRepository? _articles;
    private ICommentRepository? _comments;
    private ITagRepository? _tags;
    private IProjectRepository? _projects;
    private IMediaFileRepository? _mediaFiles;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Repository properties with lazy initialization
    public IArticleRepository Articles => 
        _articles ??= new ArticleRepository(_context);

    public ICommentRepository Comments => 
        _comments ??= new CommentRepository(_context);

    public ITagRepository Tags => 
        _tags ??= new TagRepository(_context);

    public IProjectRepository Projects => 
        _projects ??= new ProjectRepository(_context);

    public IMediaFileRepository MediaFiles => 
        _mediaFiles ??= new MediaFileRepository(_context);

    // Generic repository access
    public IRepository<T> GetRepository<T>() where T : class
    {
        return new BaseRepository<T>(_context);
    }

    // Transaction management
    public async Task<int> SaveChangesAsync()
    {
        return await SaveChangesAsync(CancellationToken.None);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle optimistic concurrency conflicts
            throw new InvalidOperationException(
                "The record you attempted to edit was modified by another user after you got the original value. " +
                "The edit operation was canceled and the current values in the database have been displayed.", ex);
        }
        catch (DbUpdateException ex)
        {
            // Handle database update exceptions
            throw new InvalidOperationException(
                "An error occurred while saving changes to the database. " +
                "Please check the data and try again.", ex);
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await BeginTransactionAsync(CancellationToken.None);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return _transaction;
    }

    public async Task CommitTransactionAsync()
    {
        await CommitTransactionAsync(CancellationToken.None);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        await RollbackTransactionAsync(CancellationToken.None);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // Bulk operations
    public async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
    {
        return await ExecuteSqlRawAsync(sql, CancellationToken.None, parameters);
    }

    public async Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken, params object[] parameters)
    {
        return await _context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }

    // Database operations
    public async Task<bool> CanConnectAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    public async Task EnsureCreatedAsync()
    {
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task EnsureDeletedAsync()
    {
        await _context.Database.EnsureDeletedAsync();
    }

    // Dispose pattern implementation
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context?.Dispose();
            _disposed = true;
        }
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }
}
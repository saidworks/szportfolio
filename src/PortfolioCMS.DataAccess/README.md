# PortfolioCMS.DataAccess

Data Access Layer for Portfolio CMS using Entity Framework Core 10 with C# 14 features.

## Features

- **EF Core 10** with C# 14 primary constructors
- **Azure SQL Database** optimized for Free tier (32 MB storage)
- **Azure Key Vault** integration with Managed Identity
- **Repository Pattern** with Unit of Work
- **Fluent API** configuration for all entities
- **Connection Resiliency** for cloud environments
- **Comprehensive Indexing** for query performance

## Architecture

### Entity Framework Context

The `ApplicationDbContext` uses C# 14 primary constructor syntax:

```csharp
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<AspNetUser>(options)
```

### Entity Configurations

All entity configurations are separated into individual files using `IEntityTypeConfiguration<T>`:

- `ArticleConfiguration.cs` - Blog articles with tags and comments
- `CommentConfiguration.cs` - User comments with moderation
- `TagConfiguration.cs` - Article tags with unique slugs
- `ArticleTagConfiguration.cs` - Many-to-many junction table
- `ProjectConfiguration.cs` - Portfolio projects
- `MediaFileConfiguration.cs` - Uploaded media files
- `AspNetUserConfiguration.cs` - User extensions

### Repository Pattern

All repositories implement the `IRepository<T>` interface with specialized repositories for each entity:

- `IArticleRepository` - Article-specific queries
- `ICommentRepository` - Comment moderation queries
- `ITagRepository` - Tag management
- `IProjectRepository` - Project queries
- `IMediaFileRepository` - Media file management

### Unit of Work

The `IUnitOfWork` interface provides transaction management across repositories:

```csharp
public interface IUnitOfWork : IDisposable
{
    IArticleRepository Articles { get; }
    ICommentRepository Comments { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

## Configuration

### Basic Setup (Development)

```csharp
// In Program.cs or Startup.cs
services.AddDataAccessServices(configuration);
```

### Azure Key Vault Integration (Production)

```csharp
// Configure Key Vault first
var keyVaultUri = configuration["KeyVault:VaultUri"];
if (!string.IsNullOrEmpty(keyVaultUri))
{
    configuration.AddAzureKeyVaultWithManagedIdentity(keyVaultUri);
}

// Then add data access services
services.AddDataAccessServices(configuration);
```

### Connection String Configuration

#### Development (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PortfolioCMS;Trusted_Connection=True;"
  }
}
```

#### Production (Azure Key Vault)

Store the connection string as a secret in Azure Key Vault:

```bash
# Create Key Vault secret
az keyvault secret set \
  --vault-name "your-keyvault-name" \
  --name "SqlConnectionString" \
  --value "Server=tcp:yourserver.database.windows.net,1433;Database=PortfolioCMS;..."
```

Configure Key Vault URI in appsettings.json:

```json
{
  "KeyVault": {
    "VaultUri": "https://your-keyvault-name.vault.azure.net/"
  }
}
```

### Managed Identity Setup

1. Enable System-Assigned Managed Identity on your Azure App Service
2. Grant the Managed Identity access to Key Vault:

```bash
az keyvault set-policy \
  --name "your-keyvault-name" \
  --object-id "<managed-identity-object-id>" \
  --secret-permissions get list
```

## Azure SQL Database Free Tier Optimizations

The configuration includes several optimizations for the 32 MB storage limit:

1. **Batch Size Limiting** - Max batch size of 42 to reduce memory usage
2. **Query Splitting** - Split complex queries to reduce memory pressure
3. **Connection Resiliency** - Automatic retry for transient failures
4. **Proper Indexing** - Composite indexes for common query patterns
5. **String Collation** - Case-insensitive collation for better performance

## Database Migrations

### Create Migration

```bash
dotnet ef migrations add MigrationName --project src/PortfolioCMS.DataAccess
```

### Apply Migration

```bash
dotnet ef database update --project src/PortfolioCMS.DataAccess
```

### Remove Last Migration

```bash
dotnet ef migrations remove --project src/PortfolioCMS.DataAccess
```

## Entity Relationships

```
AspNetUsers (1) ─────< (N) Articles
Articles (1) ─────< (N) Comments
Articles (N) ─────< (N) Tags (via ArticleTags)
Articles (1) ─────< (N) MediaFiles
Projects (1) ─────< (N) MediaFiles
AspNetUsers (1) ─────< (N) MediaFiles
```

## Performance Considerations

### Indexes

All entities have appropriate indexes for:
- Primary keys (clustered)
- Foreign keys (non-clustered)
- Frequently queried columns (non-clustered)
- Composite indexes for common query patterns

### Query Optimization

- Use `AsNoTracking()` for read-only queries
- Use `Include()` for eager loading related entities
- Use `Select()` to project only needed columns
- Avoid N+1 queries with proper eager loading

### Connection Pooling

Connection pooling is enabled by default with:
- Max pool size: 100 connections
- Min pool size: 0 connections
- Connection timeout: 30 seconds

## Security

### Managed Identity Benefits

- No connection string credentials in code
- Automatic credential rotation
- Azure AD authentication
- Audit logging in Azure

### SQL Injection Prevention

- All queries use parameterized commands
- Entity Framework prevents SQL injection by default
- Input validation at the service layer

## Testing

### Unit Tests

Use in-memory database for unit testing:

```csharp
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase(databaseName: "TestDb")
    .Options;

var context = new ApplicationDbContext(options);
```

### Integration Tests

Use SQL Server LocalDB or TestContainers for integration testing:

```csharp
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TestDb;")
    .Options;

var context = new ApplicationDbContext(options);
```

## Troubleshooting

### Connection Issues

1. Verify connection string is correct
2. Check firewall rules for Azure SQL Database
3. Ensure Managed Identity has Key Vault access
4. Check network connectivity to database server

### Migration Issues

1. Ensure migrations assembly is set correctly
2. Check for pending migrations: `dotnet ef migrations list`
3. Verify database user has schema modification permissions
4. Review migration SQL: `dotnet ef migrations script`

### Performance Issues

1. Enable query logging in development
2. Use SQL Server Profiler or Azure SQL Insights
3. Review execution plans for slow queries
4. Check index usage and fragmentation

## References

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/en-us/azure/azure-sql/)
- [Azure Key Vault Documentation](https://docs.microsoft.com/en-us/azure/key-vault/)
- [Managed Identity Documentation](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/)

# PortfolioCMS.Frontend

Blazor Server frontend application for the Portfolio CMS, orchestrated by .NET Aspire with comprehensive observability and service discovery.

## Features

### Public-Facing Pages
- **Home/Resume Page**: Portfolio showcase with projects, skills, experience, and education
- **Blog Listing**: Paginated article list with search and tag filtering
- **Article Detail**: Full article view with comment submission functionality
- **Contact Page**: Contact form with validation

### Admin Dashboard
- **Dashboard**: Overview with statistics and quick actions
- **Article Management**: Create, edit, delete articles with rich text support
- **Comment Moderation**: Approve, reject, or delete comments
- **Media Management**: Upload and manage media files
- **Project Management**: Manage portfolio projects

### Authentication
- Cookie-based authentication with JWT token integration
- Role-based authorization (Admin, Viewer)
- Protected admin routes with `[Authorize]` attribute
- Custom authentication state provider with session storage

## Architecture

### Aspire Integration
- **Service Discovery**: Automatic API endpoint resolution via Aspire
- **Resilience**: Polly retry policies via `AddStandardResilienceHandler()`
- **Telemetry**: OpenTelemetry integration for distributed tracing
- **Health Checks**: Built-in health check endpoints

### Services
- **ApiService**: Base HTTP client with error handling and logging
- **AuthenticationService**: User authentication and session management
- **ArticleService**: Article CRUD operations with caching
- **ProjectService**: Project management with caching
- **CommentService**: Comment submission and moderation
- **MediaService**: Media file upload and management

### Caching Strategy
- In-memory caching for frequently accessed data
- Cache invalidation on data modifications
- Configurable cache expiration times

## Project Structure

```
PortfolioCMS.Frontend/
├── Components/
│   ├── Layout/
│   │   ├── PublicLayout.razor      # Public-facing layout
│   │   ├── AdminLayout.razor       # Admin dashboard layout
│   │   ├── MainLayout.razor        # Default layout
│   │   └── NavMenu.razor           # Navigation menu
│   └── Pages/
│       ├── Home.razor              # Home/Resume page
│       ├── Blog.razor              # Blog listing
│       ├── ArticleDetail.razor     # Article detail view
│       ├── Contact.razor           # Contact form
│       ├── Login.razor             # Login page
│       ├── Logout.razor            # Logout handler
│       └── Admin/
│           ├── Dashboard.razor     # Admin dashboard
│           ├── Articles.razor      # Article management
│           ├── ArticleEdit.razor   # Article create/edit
│           ├── Comments.razor      # Comment moderation
│           └── Media.razor         # Media management
├── Services/
│   ├── IApiService.cs              # Base API service interface
│   ├── ApiService.cs               # HTTP client implementation
│   ├── IAuthenticationService.cs   # Auth service interface
│   ├── AuthenticationService.cs    # Auth implementation
│   ├── CustomAuthenticationStateProvider.cs  # Auth state provider
│   ├── IArticleService.cs          # Article service interface
│   ├── ArticleService.cs           # Article service implementation
│   ├── IProjectService.cs          # Project service interface
│   ├── ProjectService.cs           # Project service implementation
│   ├── ICommentService.cs          # Comment service interface
│   ├── CommentService.cs           # Comment service implementation
│   ├── IMediaService.cs            # Media service interface
│   └── MediaService.cs             # Media service implementation
├── Models/
│   ├── AuthModels.cs               # Authentication DTOs
│   ├── ArticleModels.cs            # Article DTOs
│   ├── CommentModels.cs            # Comment DTOs
│   └── ProjectModels.cs            # Project DTOs
└── Program.cs                      # Application startup

```

## Configuration

### Aspire Service Discovery
The application uses Aspire service discovery to communicate with the API project:

```csharp
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    // Aspire resolves "http://api" to the actual API service endpoint
    client.BaseAddress = new Uri("http://api");
})
.AddStandardResilienceHandler();
```

### Authentication
Cookie-based authentication with the following settings:
- Login path: `/login`
- Logout path: `/logout`
- Session expiration: 8 hours (sliding)
- Secure cookies with HttpOnly and SameSite=Strict

### Authorization Policies
- **AdminOnly**: Requires Admin role
- **Authenticated**: Requires authenticated user

## Running the Application

### Local Development with Aspire
```bash
# Run the entire solution with Aspire orchestration
dotnet run --project src/PortfolioCMS.AppHost

# Access Aspire dashboard
# http://localhost:15000 (or as configured)

# Access Frontend
# http://localhost:5001 (or as configured)
```

### Standalone (without Aspire)
```bash
cd src/PortfolioCMS.Frontend
dotnet run
```

## Dependencies

- **Microsoft.AspNetCore.Components.Authorization**: Authentication support
- **Microsoft.Extensions.Caching.Memory**: In-memory caching
- **PortfolioCMS.ServiceDefaults**: Aspire service defaults

## Future Enhancements

- Rich text editor integration (TinyMCE, Quill, or CKEditor)
- Real-time notifications with SignalR
- Advanced search with filters
- Image optimization and resizing
- Bulk operations for admin tasks
- Export functionality for articles
- Analytics dashboard
- SEO optimization tools

## Notes

- All API calls use Aspire service discovery for automatic endpoint resolution
- Resilience patterns (retry, circuit breaker) are automatically applied via Polly
- Telemetry data is automatically collected and sent to Application Insights
- Health checks are exposed at `/health` and `/alive` endpoints

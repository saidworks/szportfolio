# PortfolioCMS Tests

This project contains unit and integration tests for the Portfolio CMS application.

## Test Structure

- **Controllers/**: Unit tests for API controllers
- **Services/**: Unit tests for business logic services
- **Integration/**: Integration tests (removed due to database provider conflicts)

## Current Tests

- `HealthControllerTests`: Tests for the health check endpoints
- `ContentServiceTests`: Basic service instantiation tests

## Running Tests

```bash
dotnet test src/Tests/PortfolioCMS.Tests.csproj
```

## Test Dependencies

- xUnit: Testing framework
- Moq: Mocking framework
- FluentAssertions: Assertion library
- Microsoft.AspNetCore.Mvc.Testing: ASP.NET Core testing utilities
- Microsoft.EntityFrameworkCore.InMemory: In-memory database for testing

## Notes

The test project was recently cleaned up to remove outdated tests that didn't match the current API implementation. New tests should be added as the API evolves.
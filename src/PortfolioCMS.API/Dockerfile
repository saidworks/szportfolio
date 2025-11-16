# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["PortfolioCMS.sln", "./"]
COPY ["src/PortfolioCMS.API/PortfolioCMS.API.csproj", "src/PortfolioCMS.API/"]
COPY ["src/PortfolioCMS.DataAccess/PortfolioCMS.DataAccess.csproj", "src/PortfolioCMS.DataAccess/"]
COPY ["src/PortfolioCMS.ServiceDefaults/PortfolioCMS.ServiceDefaults.csproj", "src/PortfolioCMS.ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "src/PortfolioCMS.API/PortfolioCMS.API.csproj"

# Copy all source files
COPY . .

# Build the API project
WORKDIR "/src/src/PortfolioCMS.API"
RUN dotnet build "PortfolioCMS.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PortfolioCMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published files
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "PortfolioCMS.API.dll"]

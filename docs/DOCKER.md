# Docker Containerization Guide

This document provides detailed information about the Docker containerization strategy for the Portfolio CMS application.

## Table of Contents

- [Overview](#overview)
- [Docker Images](#docker-images)
- [Building Images](#building-images)
- [Running Containers Locally](#running-containers-locally)
- [Docker Compose](#docker-compose)
- [Security Best Practices](#security-best-practices)
- [Optimization Techniques](#optimization-techniques)
- [Troubleshooting](#troubleshooting)

## Overview

The Portfolio CMS uses multi-stage Docker builds to create optimized, production-ready container images for both the Frontend and API applications.

### Key Features

- **Multi-stage builds**: Separate build and runtime stages for smaller images
- **Non-root user**: Containers run as non-root user for security
- **Health checks**: Built-in health check endpoints
- **Layer caching**: Optimized layer ordering for faster builds
- **Security scanning**: Automated vulnerability scanning with Trivy

## Docker Images

### API Image

**Location**: `src/PortfolioCMS.API/Dockerfile`

**Base Images**:
- Build: `mcr.microsoft.com/dotnet/sdk:9.0`
- Runtime: `mcr.microsoft.com/dotnet/aspnet:9.0`

**Exposed Port**: 8080

**Health Check**: `http://localhost:8080/health`

**Image Size**: ~200 MB (runtime)

### Frontend Image

**Location**: `src/PortfolioCMS.Frontend/Dockerfile`

**Base Images**:
- Build: `mcr.microsoft.com/dotnet/sdk:9.0`
- Runtime: `mcr.microsoft.com/dotnet/aspnet:9.0`

**Exposed Port**: 8080

**Health Check**: `http://localhost:8080/health`

**Image Size**: ~210 MB (runtime)

## Building Images

### Prerequisites

- Docker Desktop or Docker Engine installed
- .NET 9.0 SDK (for local development)
- Git repository cloned

### Build Commands

#### Build API Image

```bash
# From repository root
docker build -f src/PortfolioCMS.API/Dockerfile -t portfoliocms-api:latest .

# Build with specific tag
docker build -f src/PortfolioCMS.API/Dockerfile -t portfoliocms-api:v1.0.0 .

# Build with build arguments
docker build -f src/PortfolioCMS.API/Dockerfile \
  --build-arg BUILDKIT_INLINE_CACHE=1 \
  -t portfoliocms-api:latest .
```

#### Build Frontend Image

```bash
# From repository root
docker build -f src/PortfolioCMS.Frontend/Dockerfile -t portfoliocms-frontend:latest .

# Build with specific tag
docker build -f src/PortfolioCMS.Frontend/Dockerfile -t portfoliocms-frontend:v1.0.0 .
```

### Build with Docker Buildx (Recommended)

Docker Buildx provides advanced build features including multi-platform builds and improved caching:

```bash
# Create and use buildx builder
docker buildx create --name portfoliocms-builder --use

# Build API image with cache
docker buildx build \
  -f src/PortfolioCMS.API/Dockerfile \
  -t portfoliocms-api:latest \
  --cache-from type=registry,ref=acrportfoliocms.azurecr.io/portfoliocms-api:buildcache \
  --cache-to type=registry,ref=acrportfoliocms.azurecr.io/portfoliocms-api:buildcache,mode=max \
  --load \
  .

# Build Frontend image with cache
docker buildx build \
  -f src/PortfolioCMS.Frontend/Dockerfile \
  -t portfoliocms-frontend:latest \
  --cache-from type=registry,ref=acrportfoliocms.azurecr.io/portfoliocms-frontend:buildcache \
  --cache-to type=registry,ref=acrportfoliocms.azurecr.io/portfoliocms-frontend:buildcache,mode=max \
  --load \
  .
```

## Running Containers Locally

### Run API Container

```bash
# Basic run
docker run -p 8080:8080 portfoliocms-api:latest

# Run with environment variables
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=PortfolioCMS;..." \
  portfoliocms-api:latest

# Run with volume mount for development
docker run -p 8080:8080 \
  -v $(pwd)/src/PortfolioCMS.API:/app \
  -e ASPNETCORE_ENVIRONMENT=Development \
  portfoliocms-api:latest

# Run in detached mode
docker run -d -p 8080:8080 --name portfoliocms-api portfoliocms-api:latest
```

### Run Frontend Container

```bash
# Basic run
docker run -p 8081:8080 portfoliocms-frontend:latest

# Run with API service discovery
docker run -p 8081:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e services__api__http__0=http://host.docker.internal:8080 \
  portfoliocms-frontend:latest

# Run in detached mode
docker run -d -p 8081:8080 --name portfoliocms-frontend portfoliocms-frontend:latest
```

### View Container Logs

```bash
# API logs
docker logs portfoliocms-api

# Frontend logs
docker logs portfoliocms-frontend

# Follow logs
docker logs -f portfoliocms-api
```

### Execute Commands in Container

```bash
# Open shell in API container
docker exec -it portfoliocms-api /bin/bash

# Run health check
docker exec portfoliocms-api curl http://localhost:8080/health
```

## Docker Compose

For local development with multiple services, use Docker Compose:

### docker-compose.yml

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT 1"
      interval: 10s
      timeout: 3s
      retries: 10

  api:
    build:
      context: .
      dockerfile: src/PortfolioCMS.API/Dockerfile
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=PortfolioCMS;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
    depends_on:
      sqlserver:
        condition: service_healthy
    healthcheck:
      test: curl -f http://localhost:8080/health || exit 1
      interval: 30s
      timeout: 3s
      retries: 3

  frontend:
    build:
      context: .
      dockerfile: src/PortfolioCMS.Frontend/Dockerfile
    ports:
      - "8081:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - services__api__http__0=http://api:8080
    depends_on:
      api:
        condition: service_healthy
    healthcheck:
      test: curl -f http://localhost:8080/health || exit 1
      interval: 30s
      timeout: 3s
      retries: 3

volumes:
  sqlserver-data:
```

### Running with Docker Compose

```bash
# Start all services
docker-compose up

# Start in detached mode
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v

# Rebuild images
docker-compose build

# Rebuild and start
docker-compose up --build
```

## Security Best Practices

### 1. Non-Root User

Both Dockerfiles create and use a non-root user:

```dockerfile
# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser
```

### 2. Minimal Base Images

Using official Microsoft ASP.NET runtime images which are:
- Regularly updated with security patches
- Minimal attack surface
- Optimized for .NET applications

### 3. No Secrets in Images

Never include secrets in Docker images:

```dockerfile
# ❌ BAD - Don't do this
ENV ConnectionString="Server=...;Password=secret123"

# ✅ GOOD - Use environment variables at runtime
# Set via docker run -e or docker-compose environment
```

### 4. Health Checks

Built-in health checks for container orchestration:

```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
```

### 5. Vulnerability Scanning

Automated scanning with Trivy in CI/CD:

```bash
# Scan image locally
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy:latest image portfoliocms-api:latest

# Scan with severity filter
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy:latest image --severity HIGH,CRITICAL portfoliocms-api:latest
```

## Optimization Techniques

### 1. Layer Caching

Optimize Dockerfile layer ordering:

```dockerfile
# Copy project files first (changes less frequently)
COPY ["PortfolioCMS.sln", "./"]
COPY ["src/PortfolioCMS.API/PortfolioCMS.API.csproj", "src/PortfolioCMS.API/"]

# Restore dependencies (cached if project files unchanged)
RUN dotnet restore

# Copy source code (changes more frequently)
COPY . .
```

### 2. .dockerignore

Exclude unnecessary files from build context:

```
# .dockerignore
**/bin/
**/obj/
**/.git/
**/.vs/
**/node_modules/
```

### 3. Multi-Stage Builds

Separate build and runtime stages:

```dockerfile
# Build stage - includes SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# ... build steps ...

# Runtime stage - only includes runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
# ... copy published files ...
```

### 4. Image Size Reduction

Current optimizations:
- Multi-stage builds: ~70% size reduction
- Minimal runtime image: Only ASP.NET runtime, no SDK
- No development tools in production image

### 5. Build Cache

Use BuildKit cache mounts:

```dockerfile
# Enable BuildKit
ENV DOCKER_BUILDKIT=1

# Use cache mount for NuGet packages
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore
```

## Troubleshooting

### Common Issues

#### 1. Build Context Too Large

**Problem**: Docker build is slow due to large context

**Solution**:
```bash
# Check context size
docker build --no-cache -f src/PortfolioCMS.API/Dockerfile . 2>&1 | grep "Sending build context"

# Improve .dockerignore
echo "**/bin/" >> .dockerignore
echo "**/obj/" >> .dockerignore
```

#### 2. Port Already in Use

**Problem**: Cannot start container, port already bound

**Solution**:
```bash
# Find process using port
lsof -i :8080  # macOS/Linux
netstat -ano | findstr :8080  # Windows

# Use different port
docker run -p 8082:8080 portfoliocms-api:latest
```

#### 3. Container Exits Immediately

**Problem**: Container starts and exits

**Solution**:
```bash
# Check logs
docker logs <container-id>

# Run with interactive shell
docker run -it portfoliocms-api:latest /bin/bash

# Check health
docker inspect <container-id> | grep Health
```

#### 4. Cannot Connect to Database

**Problem**: Application cannot connect to SQL Server

**Solution**:
```bash
# Use host.docker.internal for local database
-e ConnectionStrings__DefaultConnection="Server=host.docker.internal;..."

# Or use Docker network
docker network create portfoliocms-network
docker run --network portfoliocms-network ...
```

#### 5. Image Build Fails

**Problem**: Docker build fails with error

**Solution**:
```bash
# Build with verbose output
docker build --progress=plain -f src/PortfolioCMS.API/Dockerfile .

# Clear build cache
docker builder prune

# Check Dockerfile syntax
docker build --check -f src/PortfolioCMS.API/Dockerfile .
```

### Performance Issues

#### Slow Build Times

```bash
# Use BuildKit
export DOCKER_BUILDKIT=1

# Use cache from registry
docker buildx build --cache-from type=registry,ref=... --cache-to type=registry,ref=...

# Parallel builds
docker-compose build --parallel
```

#### High Memory Usage

```bash
# Limit container memory
docker run -m 512m portfoliocms-api:latest

# Monitor resource usage
docker stats
```

### Debugging

#### Enable Debug Logging

```bash
# Run with debug environment
docker run -e ASPNETCORE_ENVIRONMENT=Development \
  -e Logging__LogLevel__Default=Debug \
  portfoliocms-api:latest
```

#### Inspect Image Layers

```bash
# View image history
docker history portfoliocms-api:latest

# Inspect image
docker inspect portfoliocms-api:latest

# Use dive for detailed analysis
dive portfoliocms-api:latest
```

## Additional Resources

- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Docker Security](https://docs.docker.com/engine/security/)
- [Azure Container Registry](https://docs.microsoft.com/azure/container-registry/)

For deployment-specific information, see [DEPLOYMENT.md](./DEPLOYMENT.md).

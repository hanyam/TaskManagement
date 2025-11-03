# Task Management API - Deployment Guide

**Version:** 1.0  
**Last Updated:** 2024-01-15

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Local Development Setup](#local-development-setup)
3. [Database Setup](#database-setup)
4. [Production Deployment](#production-deployment)
5. [Docker Deployment](#docker-deployment)
6. [CI/CD Considerations](#cicd-considerations)

---

## Prerequisites

### .NET SDK Requirements

**Required Version:**
- .NET 8.0 SDK or later

**Verify Installation:**
```bash
dotnet --version
# Should output: 8.0.x or higher
```

**Installation:**
- Download from https://dotnet.microsoft.com/download
- Or use package manager (apt, yum, brew, etc.)

### Database Requirements

**Supported Databases:**
- SQL Server 2019 or later
- SQL Server Express
- Azure SQL Database
- SQL Server LocalDB (development only)

**Connection Requirements:**
- TCP/IP enabled
- SQL Authentication or Windows Authentication
- Sufficient permissions for schema creation and data access

### Azure AD Setup

**Required:**
- Azure AD tenant
- App registration created
- Client ID and Client Secret
- Configured redirect URIs (if needed)

**Setup Steps:**
1. Go to Azure Portal → Azure Active Directory
2. Create App Registration
3. Note Client ID and Tenant ID
4. Create Client Secret
5. Configure API permissions (if needed)

---

## Local Development Setup

### Clone Repository

```bash
git clone https://github.com/your-org/TaskManagement.git
cd TaskManagement
```

### Restore Packages

```bash
dotnet restore
```

### Configure Settings

**1. Update `appsettings.Development.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "SecretKey": "YourDevelopmentSecretKeyAtLeast32CharactersLong!",
    "Issuer": "TaskManagement.Api",
    "Audience": "TaskManagement.Client",
    "ExpiryInHours": 24
  },
  "AzureAd": {
    "Issuer": "https://login.microsoftonline.com/your-tenant-id/v2.0",
    "ClientId": "your-dev-client-id",
    "ClientSecret": "your-dev-client-secret"
  }
}
```

**2. Generate JWT Secret Key:**
```bash
# PowerShell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))

# Or use dotnet user-secrets
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key" --project src/TaskManagement.Api
```

### Run Database Migrations

**Ensure Database Created:**
```bash
cd src/TaskManagement.Api
dotnet ef database update --project ../TaskManagement.Infrastructure
```

**Or use EnsureCreated (development only):**
- Application automatically creates database on startup if configured

### Start Application

```bash
cd src/TaskManagement.Api
dotnet run
```

**Access Application:**
- API: https://localhost:7000
- Swagger UI: https://localhost:7000 (root path)

---

## Database Setup

### SQL Server Installation

**Windows:**
- Download SQL Server Express or Developer Edition
- Install with default instance
- Enable SQL Server Authentication (if needed)

**Linux:**
```bash
# Install SQL Server
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
sudo add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/20.04/mssql-server-2022.list)"
sudo apt-get update
sudo apt-get install -y mssql-server
```

### Database Creation

**Using SQL Server Management Studio:**
```sql
CREATE DATABASE TaskManagementDb;
GO
```

**Using Command Line:**
```bash
sqlcmd -S localhost -U sa -P YourPassword -Q "CREATE DATABASE TaskManagementDb"
```

### Migration Execution

**Using EF Core CLI:**
```bash
cd src/TaskManagement.Api
dotnet ef database update --project ../TaskManagement.Infrastructure
```

**Using Package Manager Console:**
```powershell
Update-Database -Project TaskManagement.Infrastructure -StartupProject TaskManagement.Api
```

**Verify Migration:**
```sql
SELECT * FROM [__EFMigrationsHistory];
```

---

## Production Deployment

### Build Process

**Build for Production:**
```bash
dotnet publish src/TaskManagement.Api/TaskManagement.Api.csproj \
  -c Release \
  -o ./publish
```

**Build Output:**
- Compiled DLLs
- Configuration files
- Dependencies

### Environment Variables

**Required Variables:**
```bash
ConnectionStrings__DefaultConnection="Server=prod-server;Database=TaskManagementDb;..."
Jwt__SecretKey="your-production-secret-key"
Jwt__Issuer="https://api.taskmanagement.com"
Jwt__Audience="https://app.taskmanagement.com"
AzureAd__Issuer="https://login.microsoftonline.com/tenant-id/v2.0"
AzureAd__ClientId="your-production-client-id"
AzureAd__ClientSecret="your-production-client-secret"
ASPNETCORE_ENVIRONMENT="Production"
```

**Set Environment Variables:**
```bash
# Linux/Mac
export ConnectionStrings__DefaultConnection="..."
export Jwt__SecretKey="..."

# Windows (PowerShell)
$env:ConnectionStrings__DefaultConnection = "..."
$env:Jwt__SecretKey = "..."
```

### Database Migration Strategy

**Pre-Deployment:**
1. Backup production database
2. Review migration scripts
3. Test migrations on staging environment

**Deployment:**
```bash
# Run migrations
dotnet ef database update --project src/TaskManagement.Infrastructure \
  --startup-project src/TaskManagement.Api \
  --connection "Server=prod-server;Database=TaskManagementDb;..."
```

**Post-Deployment:**
1. Verify migration applied successfully
2. Check database schema
3. Verify application functionality

### Health Checks

**Health Check Endpoint:**
```
GET /health
```

**Response:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "db": {
      "status": "Healthy",
      "description": "Entity Framework health check",
      "duration": "00:00:00.0123456"
    }
  }
}
```

**Monitoring:**
- Set up health check monitoring
- Alert on unhealthy status
- Monitor response times

---

## Docker Deployment

### Dockerfile

**Create Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/TaskManagement.Api/TaskManagement.Api.csproj", "src/TaskManagement.Api/"]
COPY ["src/TaskManagement.Application/TaskManagement.Application.csproj", "src/TaskManagement.Application/"]
COPY ["src/TaskManagement.Domain/TaskManagement.Domain.csproj", "src/TaskManagement.Domain/"]
COPY ["src/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj", "src/TaskManagement.Infrastructure/"]
RUN dotnet restore "src/TaskManagement.Api/TaskManagement.Api.csproj"
COPY . .
WORKDIR "/src/src/TaskManagement.Api"
RUN dotnet build "TaskManagement.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManagement.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement.Api.dll"]
```

### docker-compose Setup

**docker-compose.yml:**
```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:80"
      - "8443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=TaskManagementDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
      - Jwt__SecretKey=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
    depends_on:
      - db
  
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql

volumes:
  sql-data:
```

**Run with Docker Compose:**
```bash
docker-compose up -d
```

### Build and Run Docker Image

```bash
# Build image
docker build -t taskmanagement-api .

# Run container
docker run -d \
  -p 8080:80 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Jwt__SecretKey="..." \
  taskmanagement-api
```

---

## CI/CD Considerations

### Build Pipelines

**Azure DevOps Example:**
```yaml
trigger:
  branches:
    include:
      - main
      - dev

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    packageType: 'sdk'
    version: '8.x'

- task: DotNetCoreCLI@2
  inputs:
    command: 'restore'
    projects: '**/*.csproj'

- task: DotNetCoreCLI@2
  inputs:
    command: 'build'
    projects: '**/*.csproj'

- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: 'tests/**/*.csproj'
```

### Test Execution

**Run Tests in Pipeline:**
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: 'tests/**/*.csproj'
    arguments: '--collect:"XPlat Code Coverage"'
```

### Deployment Automation

**Azure DevOps Deployment:**
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'publish'
    projects: 'src/TaskManagement.Api/TaskManagement.Api.csproj'
    arguments: '-c Release -o $(Build.ArtifactStagingDirectory)'

- task: AzureWebApp@1
  inputs:
    azureSubscription: 'your-subscription'
    appName: 'taskmanagement-api'
    package: '$(Build.ArtifactStagingDirectory)'
```

**GitHub Actions Example:**
```yaml
name: Deploy

on:
  push:
    branches:
      - main

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build
      - run: dotnet test
      - run: dotnet publish -c Release
```

---

## See Also

- [Configuration Guide](CONFIGURATION.md) - Configuration options
- [Security Documentation](SECURITY.md) - Security best practices
- [Architecture Documentation](ARCHITECTURE.md) - System architecture


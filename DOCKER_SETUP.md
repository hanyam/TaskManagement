# Docker Setup Guide

Complete guide for running the Task Management application with Docker Compose.

## 🚀 Quick Start

### Prerequisites

- Docker Desktop installed and running
- Ports 1433, 3000, and 5000 available

### Start the Application

```bash
# From the repository root
docker compose up -d

# View logs
docker compose logs -f
```

**Access the application:**
- Web UI: `http://localhost:3000`
- Backend API: `http://localhost:5000`
- SQL Server: `localhost:1433`

---

## 📋 Configuration Options

### Option 1: Manual Token Entry (Default)

The application works out of the box with manual Azure AD token entry.

**How to use:**
1. Get an Azure AD token from another source (Postman, Azure CLI, etc.)
2. Go to `http://localhost:3000/en/sign-in`
3. Paste the token in the input field
4. Click "Sign In"

**No additional configuration needed!**

### Option 2: Azure AD SSO (Recommended)

Enable the "Continue with Azure AD" button for seamless authentication.

#### Step 1: Get Azure AD Credentials

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Select your app (or create one)
4. Copy these values:
   - **Application (client) ID**
   - **Directory (tenant) ID**

#### Step 2: Configure Redirect URI in Azure Portal

1. In your App Registration, go to **Authentication**
2. Under **Platform configurations** > **Single-page application**
3. Add redirect URI: `http://localhost:3000`
4. Click **Save**

#### Step 3: Add API Permissions

1. Go to **API permissions**
2. Add these permissions:
   - `openid` (Microsoft Graph, delegated)
   - `profile` (Microsoft Graph, delegated)
   - `email` (Microsoft Graph, delegated)
   - `api://your-backend-client-id/.default` (your backend API)
3. Click **Grant admin consent**

#### Step 4: Update docker-compose.yml

Edit `docker-compose.yml` and uncomment/update the Azure AD build args:

```yaml
taskmanagement.web:
  build:
    args:
      - NEXT_PUBLIC_API_BASE_URL=http://localhost:5000
      - NEXT_PUBLIC_APP_NAME=Task Management Console
      - NEXT_PUBLIC_AZURE_AD_CLIENT_ID=your-client-id-here        # ← Add your values
      - NEXT_PUBLIC_AZURE_AD_TENANT_ID=your-tenant-id-here        # ← Add your values
      - NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=http://localhost:3000
      - NEXT_PUBLIC_AZURE_AD_SCOPES=api://your-backend-id/.default,openid,profile,email
```

#### Step 5: Rebuild and Restart

```bash
# Rebuild the web container with new configuration
docker compose build taskmanagement.web

# Restart all containers
docker compose up -d

# Verify the web container started successfully
docker compose logs taskmanagement.web
```

#### Step 6: Test Azure AD Login

1. Go to `http://localhost:3000/en/sign-in`
2. The "Continue with Azure AD" button should now be **enabled**
3. Click it to test SSO

---

## 🏗️ Architecture

### Container Overview

```
┌─────────────────────────────────────────────────────────────┐
│                         Host Machine                         │
│  (Windows/Mac/Linux)                                         │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Browser (http://localhost:3000)                      │  │
│  └────────────┬─────────────────────────────────────────┘  │
│               │                                              │
│               │ Calls API at http://localhost:5000          │
│               ↓                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Docker Network (taskmanagement_default)             │  │
│  │                                                       │  │
│  │  ┌─────────────────┐  ┌──────────────┐  ┌─────────┐ │  │
│  │  │ taskmanagement  │  │ taskmanagement│  │ SQL     │ │  │
│  │  │ -web            │  │ -api          │  │ Server  │ │  │
│  │  │                 │  │               │  │         │ │  │
│  │  │ Port: 3000      │  │ Port: 8080    │  │Port:1433│ │  │
│  │  └────────┬────────┘  └───────┬───────┘  └────┬────┘ │  │
│  │           │                    │               │      │  │
│  │           │  API calls         │               │      │  │
│  │           └───────────────────►│               │      │  │
│  │                                │  DB queries   │      │  │
│  │                                └──────────────►│      │  │
│  │                                                       │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                              │
│  Port Mappings:                                             │
│  - 3000:3000  → Web UI                                      │
│  - 5000:8080  → Backend API                                 │
│  - 1433:1433  → SQL Server                                  │
└─────────────────────────────────────────────────────────────┘
```

### Services

| Service | Container Name | Internal Port | External Port | Image |
|---------|----------------|---------------|---------------|-------|
| Web UI | `taskmanagement-web` | 3000 | 3000 | `taskmanagement.web` |
| Backend API | `taskmanagement-api` | 8080 | 5000 | `taskmanagementapi` |
| Database | `taskmanagement-sqlserver` | 1433 | 1433 | `mcr.microsoft.com/mssql/server:2022-latest` |

---

## 🔧 Environment Variables

### Build-Time Variables (Set in docker-compose.yml → build → args)

These are **baked into the JavaScript bundle** at build time:

```yaml
args:
  - NEXT_PUBLIC_API_BASE_URL=http://localhost:5000  # API URL for browser
  - NEXT_PUBLIC_APP_NAME=Task Management Console
  - NEXT_PUBLIC_AZURE_AD_CLIENT_ID=...             # Optional
  - NEXT_PUBLIC_AZURE_AD_TENANT_ID=...             # Optional
  - NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=...          # Optional
  - NEXT_PUBLIC_AZURE_AD_SCOPES=...                # Optional
```

**Important:** Changes to these require rebuilding the web container:

```bash
docker compose build taskmanagement.web
docker compose up -d
```

### Runtime Variables (Set in docker-compose.yml → environment)

These can be changed without rebuilding:

**API Service:**
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - ASPNETCORE_HTTP_PORTS=8080
  - ASPNETCORE_URLS=http://+:8080
  - ConnectionStrings__DefaultConnection=Server=sqlserver;...
```

**Web Service:**
```yaml
environment:
  - NODE_ENV=production
```

---

## 🛠️ Common Tasks

### View Logs

```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f taskmanagement.web
docker compose logs -f taskmanagement.api
docker compose logs -f sqlserver

# Last N lines
docker compose logs --tail=50 taskmanagement.api
```

### Restart Services

```bash
# Restart all
docker compose restart

# Restart specific service
docker compose restart taskmanagement.web
docker compose restart taskmanagement.api
```

### Rebuild Services

```bash
# Rebuild all
docker compose build

# Rebuild specific service
docker compose build taskmanagement.web
docker compose build taskmanagement.api

# Rebuild and restart
docker compose up -d --build
```

### Stop and Remove Containers

```bash
# Stop containers (keeps data)
docker compose stop

# Stop and remove containers (keeps data)
docker compose down

# Stop, remove containers, and DELETE all data
docker compose down -v
```

### Access Container Shell

```bash
# Web container
docker exec -it taskmanagement-web sh

# API container
docker exec -it taskmanagement-api sh

# SQL Server container
docker exec -it taskmanagement-sqlserver /bin/bash
```

### Database Access

```bash
# Connect to SQL Server
docker exec -it taskmanagement-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" -C

# Query database
SELECT * FROM Tasks.Tasks;
GO
```

---

## 🐛 Troubleshooting

### Issue: Web UI shows "Continue with Azure AD" button disabled

**Cause:** Azure AD configuration not set in build args.

**Solution:**
1. Uncomment and set Azure AD variables in `docker-compose.yml` (under `build → args`)
2. Rebuild: `docker compose build taskmanagement.web`
3. Restart: `docker compose up -d`

---

### Issue: "Connection refused" when calling API

**Cause:** API container not running or port mapping incorrect.

**Solution:**
```bash
# Check container status
docker compose ps

# Check API logs
docker compose logs taskmanagement.api

# Verify API is listening
curl http://localhost:5000/health
# or in PowerShell:
Invoke-WebRequest -Uri http://localhost:5000/health
```

---

### Issue: "HTTPS endpoint configuration error"

**Cause:** API trying to use HTTPS without a certificate.

**Solution:** Already fixed! The API now uses HTTP only (port 8080 internally, 5000 externally).

---

### Issue: Database connection errors

**Cause:** SQL Server not ready or wrong connection string.

**Solution:**
```bash
# Check SQL Server health
docker compose ps sqlserver

# Check SQL Server logs
docker compose logs sqlserver

# Wait for SQL Server to be healthy (takes 30-60 seconds on first start)
docker compose up -d
# Wait...
docker compose ps  # Check if sqlserver shows "(healthy)"
```

---

### Issue: "CORS error" in browser

**Cause:** API CORS policy not allowing localhost:3000.

**Solution:** Already configured! CORS allows `http://localhost:3000` in `src/TaskManagement.Api/DependencyInjection.cs`.

---

### Issue: Changes to `.env.local` not reflected

**Cause:** Docker uses build args, not `.env.local`.

**Solution:** 
- For Docker: Set variables in `docker-compose.yml` under `build → args`
- For local development: Use `.env.local`

---

## 🔄 Development Workflow

### Local Development (Without Docker)

1. **Backend:**
   ```bash
   cd src/TaskManagement.Api
   dotnet run
   # Runs on http://localhost:5010
   ```

2. **Frontend:**
   ```bash
   cd web
   cp .env.local.example .env.local
   # Edit .env.local with your values
   npm install
   npm run dev
   # Runs on http://localhost:3000
   ```

### Docker Development

1. **Make changes to code**

2. **Rebuild affected service:**
   ```bash
   # Backend changes
   docker compose build taskmanagement.api
   docker compose up -d

   # Frontend changes
   docker compose build taskmanagement.web
   docker compose up -d
   ```

3. **Test**

---

## 📊 Health Checks

### API Health Endpoint

```bash
# PowerShell
Invoke-WebRequest -Uri http://localhost:5000/health

# Expected response: "Healthy"
```

### SQL Server Health

```bash
docker compose ps

# Look for "(healthy)" status next to sqlserver
```

### Web UI

Navigate to `http://localhost:3000` - should load the sign-in page.

---

## 🚀 Production Deployment

### Update Configuration

1. **Change API URL to production:**
   ```yaml
   args:
     - NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com
   ```

2. **Set production Azure AD redirect URI:**
   ```yaml
   args:
     - NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=https://yourdomain.com
   ```

3. **Update connection string:**
   ```yaml
   environment:
     - ConnectionStrings__DefaultConnection=Server=production-sql;...
   ```

### Security Considerations

- **Use HTTPS:** Set up a reverse proxy (nginx, Traefik) to handle SSL
- **Change SQL Server password:** Don't use `YourStrong@Passw0rd` in production
- **Use secrets:** Store sensitive values in Docker secrets or environment variables
- **Enable authentication:** Ensure SQL Server uses strong authentication
- **Data persistence:** Use Docker volumes for SQL Server data (already configured)

---

## 📚 Additional Documentation

- [Azure AD Setup Guide](web/AZURE_AD_QUICKSTART.md)
- [Direct Backend Authentication](web/docs/DIRECT_BACKEND_AUTH.md)
- [Environment Variables Reference](web/docs/ENVIRONMENT_VARIABLES.md)
- [Web UI README](web/README.md)

---

## 🆘 Getting Help

If you encounter issues:

1. Check container logs: `docker compose logs -f`
2. Verify container status: `docker compose ps`
3. Review this troubleshooting guide
4. Check the detailed documentation in `web/docs/`

---

**Last Updated:** Based on current Docker configuration with HTTP-only API and direct backend authentication.



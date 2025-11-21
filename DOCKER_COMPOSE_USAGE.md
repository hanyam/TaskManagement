# Docker Compose Project Name Configuration

## Quick Reference

To change the Docker Compose project name, use one of these methods:

**IMPORTANT**: If the `name` field in `docker-compose.yml` is not working, use Method 1 (command line flag) or Method 2 (environment variable) instead.

### Method 1: Command Line Flag (Recommended - Most Reliable)

Use the `-p` or `--project-name` flag when running docker compose commands:

**Windows PowerShell:**
```powershell
# Start with custom project name
docker compose -p TaskManagement up

# Build and start
docker compose -p TaskManagement up --build

# Stop containers
docker compose -p TaskManagement down

# View containers
docker compose -p TaskManagement ps
```

**Windows CMD:**
```cmd
docker compose -p TaskManagement up
docker compose -p TaskManagement up --build
docker compose -p TaskManagement down
docker compose -p TaskManagement ps
```

**Linux/Mac:**
```bash
docker compose -p TaskManagement up
docker compose -p TaskManagement up --build
docker compose -p TaskManagement down
docker compose -p TaskManagement ps
```

**Note:** You must use the same `-p` flag for all commands (up, down, ps, logs, etc.)

### Method 2: Environment Variable via .env File (Persistent - Recommended)

Create a `.env` file in the project root (copy from `.env.example`):

```env
COMPOSE_PROJECT_NAME=TaskManagement
```

Docker Compose will automatically read this file and use the project name.

**Note:** The `.env` file is already in `.gitignore`, so it won't be committed to version control.

**Alternative - Set in PowerShell (Current Session Only):**
```powershell
$env:COMPOSE_PROJECT_NAME = "TaskManagement"
docker compose up
```

**Alternative - Set in CMD (Current Session Only):**
```cmd
set COMPOSE_PROJECT_NAME=TaskManagement
docker compose up
```

**Alternative - Set in Linux/Mac (Current Session Only):**
```bash
export COMPOSE_PROJECT_NAME=TaskManagement
docker compose up
```

### Method 3: Add `name` Field to docker-compose.yml

The `name` field is already set in `docker-compose.yml`:

```yaml
name: TaskManagement

services:
  sqlserver:
    # ... rest of config
```

**Note:** This method requires Docker Compose v2.20+ or v3.8+. If it's not working, use Method 1 or Method 2 instead.

## Current Default Behavior

By default, Docker Compose uses the directory name as the project name. For example:
- Directory: `TaskManagement` → Project name: `taskmanagement` (lowercased)
- Containers will be prefixed: `taskmanagement-sqlserver`, `taskmanagement-api`, etc.

**However**, if Docker Compose can't determine the directory name, it may generate a random name like `dockercompose2188465437773426356`. To fix this, use Method 1 or Method 2 above.

## Examples

### Example 1: Development Environment
```powershell
# Windows PowerShell
docker compose -p TaskManagement up --build
```

### Example 2: Using .env File
Create a `.env` file with:
```env
COMPOSE_PROJECT_NAME=TaskManagement
```

Then run:
```powershell
docker compose up --build
```

### Example 3: Multiple Instances
```powershell
# Instance 1
docker compose -p TaskManagement-Dev up

# Instance 2 (different ports)
docker compose -p TaskManagement-Prod up
```

## Viewing Current Project Name

To see what project name is being used:

```bash
docker compose ps
```

The project name appears in container names and network names.

## Important Notes

1. **Container Names**: If you specify `container_name` in docker-compose.yml, it will override the project-prefixed name
2. **Networks**: Networks are prefixed with the project name
3. **Volumes**: Volumes are prefixed with the project name
4. **Consistency**: Always use the same project name for related commands (up, down, ps, logs, etc.)


# Task Management API - Configuration Guide

**Version:** 1.0  
**Last Updated:** 2024-01-15

## Table of Contents

1. [Application Settings](#application-settings)
2. [JWT Configuration](#jwt-configuration)
3. [Azure AD Configuration](#azure-ad-configuration)
4. [Business Logic Configuration](#business-logic-configuration)
5. [Environment-Specific Configuration](#environment-specific-configuration)

---

## Application Settings

### ConnectionStrings

**Development (`appsettings.Development.json`):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**Production:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server.database.windows.net;Database=TaskManagementDb;User Id=your-user;Password=your-password;Encrypt=true;TrustServerCertificate=false"
  }
}
```

**Azure SQL Database:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=TaskManagementDb;Persist Security Info=False;User ID=your-user;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### Logging Configuration

**Serilog Configuration:**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/taskmanagement-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

**Log Levels:**
- `Verbose`: Detailed diagnostic information
- `Debug`: Debug-level information
- `Information`: General information (default)
- `Warning`: Warning messages
- `Error`: Error messages
- `Fatal`: Critical errors

### CORS Settings

**Development:**
```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:4200"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["Authorization", "Content-Type"]
  }
}
```

**Production:**
```json
{
  "Cors": {
    "AllowedOrigins": ["https://app.taskmanagement.com"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["Authorization", "Content-Type"],
    "AllowCredentials": true
  }
}
```

---

## JWT Configuration

### SecretKey Generation

**Generate a secure secret key:**
```bash
# Using PowerShell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))

# Using OpenSSL
openssl rand -base64 32

# Using .NET
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key-here"
```

**Minimum Requirements:**
- At least 32 characters (256 bits)
- Random and unpredictable
- Stored securely (not in code or version control)

### JWT Settings

**Configuration:**
```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "TaskManagement.Api",
    "Audience": "TaskManagement.Client",
    "ExpiryInHours": 1
  }
}
```

**Property Descriptions:**
- `SecretKey`: Secret key for signing JWT tokens (minimum 32 characters)
- `Issuer`: Token issuer identifier (typically API URL)
- `Audience`: Token audience identifier (typically client application name)
- `ExpiryInHours`: Token expiration time in hours (default: 1)

**Environment Variables:**
```bash
Jwt__SecretKey="YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
Jwt__Issuer="TaskManagement.Api"
Jwt__Audience="TaskManagement.Client"
Jwt__ExpiryInHours=1
```

### Token Validation Settings

Configured in `Program.cs` via `TokenValidationParameters`:
- `ValidateIssuer`: true
- `ValidateAudience`: true
- `ValidateLifetime`: true
- `ValidateIssuerSigningKey`: true

---

## Azure AD Configuration

### Tenant Setup

**Required Information:**
1. Azure AD Tenant ID
2. Application (App Registration) details
3. Client ID
4. Client Secret (if using client credentials)

### App Registration

**Steps:**
1. Go to Azure Portal → Azure Active Directory → App registrations
2. Click "New registration"
3. Configure:
   - Name: "Task Management API"
   - Supported account types: Single tenant or Multi-tenant
   - Redirect URI: (if needed)
4. After registration, note:
   - Application (client) ID
   - Directory (tenant) ID

### Client ID and Secret

**Creating Client Secret:**
1. Go to App Registration → Certificates & secrets
2. Click "New client secret"
3. Configure:
   - Description: "API Client Secret"
   - Expires: Choose expiration (6, 12, or 24 months)
4. Copy the secret value (shown only once)

### Configuration

**appsettings.json:**
```json
{
  "AzureAd": {
    "Issuer": "https://login.microsoftonline.com/{tenant-id}/v2.0",
    "ClientId": "your-azure-ad-client-id",
    "ClientSecret": "your-azure-ad-client-secret",
    "TenantId": "your-azure-ad-tenant-id"
  }
}
```

**Environment Variables:**
```bash
AzureAd__Issuer="https://login.microsoftonline.com/{tenant-id}/v2.0"
AzureAd__ClientId="your-azure-ad-client-id"
AzureAd__ClientSecret="your-azure-ad-client-secret"
AzureAd__TenantId="your-azure-ad-tenant-id"
```

**Placeholders:**
- `{tenant-id}`: Your Azure AD tenant ID
- Replace all placeholders with actual values

---

## Business Logic Configuration

### ReminderOptions

**Configuration:**
```json
{
  "Reminder": {
    "UseDayThresholds": false,
    "Thresholds": {
      "Critical": 0.90,
      "High": 0.75,
      "Medium": 0.50,
      "Low": 0.25
    },
    "DayThresholds": {
      "Critical": 1,
      "High": 3,
      "Medium": 7,
      "Low": 14
    }
  }
}
```

**Property Descriptions:**
- `UseDayThresholds`: If true, use day-based thresholds instead of percentage-based
- `Thresholds`: Percentage thresholds (0-1.0) for reminder levels
  - `Critical`: 90% of time elapsed
  - `High`: 75% of time elapsed
  - `Medium`: 50% of time elapsed
  - `Low`: 25% of time elapsed
- `DayThresholds`: Fixed day thresholds (days before due date)
  - `Critical`: 1 day before due date
  - `High`: 3 days before due date
  - `Medium`: 7 days before due date
  - `Low`: 14 days before due date

**Calculation Method:**
- If `UseDayThresholds` is true: Uses day thresholds
- If `UseDayThresholds` is false: Uses percentage thresholds (requires `CreatedAt` date)

### ExtensionPolicyOptions

**Configuration:**
```json
{
  "ExtensionPolicy": {
    "MaxExtensionRequestsPerTask": 3,
    "MinDaysBeforeDueDate": 1,
    "MaxExtensionDays": 30,
    "RequiresManagerApproval": true,
    "AutoApproveIfConditionsMet": false
  }
}
```

**Property Descriptions:**
- `MaxExtensionRequestsPerTask`: Maximum number of extension requests allowed per task (default: 3)
- `MinDaysBeforeDueDate`: Minimum days before due date when extension can be requested (default: 1, 0 = any time)
- `MaxExtensionDays`: Maximum extension days allowed from current due date (default: 30)
- `RequiresManagerApproval`: Whether manager approval is required (default: true)
- `AutoApproveIfConditionsMet`: Whether to auto-approve if all conditions are met (default: false, not currently implemented)

---

## Environment-Specific Configuration

### Development Settings

**appsettings.Development.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  },
  "Jwt": {
    "SecretKey": "DevelopmentSecretKeyAtLeast32CharactersLong!",
    "Issuer": "TaskManagement.Api",
    "Audience": "TaskManagement.Client",
    "ExpiryInHours": 24
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:4200"]
  }
}
```

**Features:**
- LocalDB for database
- Debug logging enabled
- Longer token expiration for development
- Permissive CORS for local development

### Production Settings

**appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "#{ConnectionString}#"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  },
  "Jwt": {
    "SecretKey": "#{JwtSecretKey}#",
    "Issuer": "https://api.taskmanagement.com",
    "Audience": "https://app.taskmanagement.com",
    "ExpiryInHours": 1
  },
  "Cors": {
    "AllowedOrigins": ["https://app.taskmanagement.com"]
  }
}
```

**Features:**
- Secure connection string (from Azure Key Vault)
- Warning-level logging
- Short token expiration
- Restricted CORS
- Placeholders for secret injection

### Testing Settings

**appsettings.json (Test Project):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb_Test;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "SecretKey": "TestSecretKeyAtLeast32CharactersLong!",
    "Issuer": "TaskManagement.Api.Test",
    "Audience": "TaskManagement.Client.Test",
    "ExpiryInHours": 1
  }
}
```

**Features:**
- In-memory or test database
- Test-specific JWT settings
- Minimal logging

---

## Configuration Best Practices

### Secret Management

**Development:**
- Use `appsettings.Development.json` (excluded from version control)
- Use User Secrets for sensitive data:
  ```bash
  dotnet user-secrets set "Jwt:SecretKey" "your-secret"
  ```

**Production:**
- Use environment variables
- Use Azure Key Vault (recommended)
- Use secure configuration providers
- Never commit secrets to version control

### Configuration Validation

**Validate on Startup:**
- Check required configuration values
- Validate connection strings
- Verify Azure AD configuration
- Ensure JWT secret key is set

**Example:**
```csharp
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
if (string.IsNullOrEmpty(jwtOptions?.SecretKey))
    throw new InvalidOperationException("JWT SecretKey is required");
```

### Environment Variables

**Format:**
- Use double underscore (`__`) for nested properties
- Example: `Jwt__SecretKey`, `AzureAd__ClientId`

**Loading:**
```bash
# Linux/Mac
export Jwt__SecretKey="your-secret-key"

# Windows (PowerShell)
$env:Jwt__SecretKey = "your-secret-key"
```

---

## See Also

- [Security Documentation](SECURITY.md) - Security best practices
- [Deployment Guide](DEPLOYMENT.md) - Deployment configuration
- [Architecture Documentation](ARCHITECTURE.md) - System architecture


# Task Management API - Security & Authorization Documentation

**Version:** 1.0  
**Last Updated:** 2024-01-15

## Table of Contents

1. [Authentication Flow](#authentication-flow)
2. [Authorization Model](#authorization-model)
3. [Security Best Practices](#security-best-practices)
4. [Error Handling Security](#error-handling-security)
5. [Configuration Security](#configuration-security)

---

## Authentication Flow

### Azure AD Integration

The application integrates with Azure AD for enterprise authentication.

**Flow:**
1. User authenticates with Azure AD (external to application)
2. Azure AD returns access token
3. Client sends Azure AD token to `/api/authentication/authenticate`
4. Application validates Azure AD token
5. Application extracts user claims from Azure AD token
6. Application finds or creates user in database
7. Application generates custom JWT token with additional claims
8. Client receives JWT token
9. Client uses JWT token for subsequent API requests

### JWT Token Generation

**Token Claims:**
- `user_id`: User's unique identifier (Guid)
- `email`: User's email address
- `name`: User's display name
- `role`: User role (Employee, Manager, Admin)
- `sub`: Subject (user's email)
- `iss`: Issuer (from configuration)
- `aud`: Audience (from configuration)
- `exp`: Expiration timestamp
- `iat`: Issued at timestamp

**Token Structure:**
```json
{
  "user_id": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "name": "John Doe",
  "role": "Employee",
  "sub": "user@example.com",
  "iss": "TaskManagement.Api",
  "aud": "TaskManagement.Client",
  "exp": 1704123600,
  "iat": 1704120000
}
```

### Token Validation

**Middleware Configuration:**
- Validates token signature using secret key
- Validates issuer (`iss` claim)
- Validates audience (`aud` claim)
- Validates expiration (`exp` claim)
- Validates lifetime

**Invalid Token Handling:**
- Returns `401 Unauthorized` status
- Error message: "Unauthorized access"
- Token not processed further

### Claims Structure

**Required Claims:**
- `user_id`: Extracted via `User.FindFirst("user_id")?.Value`
- Used for identifying current user in controllers
- Validated to be a valid Guid

**Role Claims:**
- `role`: Extracted via `User.FindFirst("role")?.Value` or `User.IsInRole()`
- Used for authorization checks
- Valid values: "Employee", "Manager", "Admin"

---

## Authorization Model

### Role-Based Access Control (RBAC)

The application implements role-based access control with three roles:

#### Employee Role
**Permissions:**
- Create tasks
- View assigned tasks
- Update assigned task details
- Update task progress
- Accept/reject assigned tasks
- Request deadline extensions
- View dashboard statistics

**Restrictions:**
- Cannot assign tasks to others
- Cannot accept progress updates
- Cannot approve extension requests
- Cannot mark tasks as completed
- Cannot view all tasks (only assigned)

#### Manager Role
**Permissions:**
- All Employee permissions
- Assign tasks to employees
- Reassign tasks
- Accept task progress updates
- Approve/reject deadline extension requests
- Mark tasks as completed
- View all tasks (not just assigned)

**Special Actions:**
- Delegation management
- Progress review and acceptance
- Extension request review
- Task completion

#### Admin Role
**Permissions:**
- All Manager permissions
- System administration (if implemented)
- User management (if implemented)
- Override business rules (if implemented)

### Endpoint Protection

**Attribute-Based Authorization:**
```csharp
[Authorize] // Requires authentication
[Authorize(Roles = "Manager")] // Requires Manager role
[Authorize(Roles = "Employee,Manager")] // Requires Employee OR Manager role
```

**Controller-Level:**
- `TasksController`: `[Authorize]` - All endpoints require authentication
- `DashboardController`: `[Authorize]` - All endpoints require authentication
- `AuthenticationController`: `[AllowAnonymous]` - Public endpoint

**Endpoint-Level:**
- `/api/tasks/{id}/assign`: Manager only
- `/api/tasks/{id}/progress/accept`: Manager only
- `/api/tasks/{id}/complete`: Manager only
- `/api/tasks/{id}/extension-request/{requestId}/approve`: Manager only

### Handler-Level Authorization

**Additional Checks:**
- User ID validation in handlers
- Assignment verification
- Ownership verification
- Business rule enforcement

**Example:**
```csharp
// Verify user is assigned to task
var assignment = await _context.Set<TaskAssignment>()
    .FirstOrDefaultAsync(a => a.TaskId == task.Id && a.UserId == request.UpdatedById);
if (assignment == null)
    return Result<TaskProgressDto>.Failure(TaskErrors.UnauthorizedAccess);
```

---

## Security Best Practices

### Token Storage

**Client-Side:**
- Store JWT token in secure storage (e.g., HttpOnly cookie, secure localStorage)
- Never expose token in URL parameters
- Implement token refresh mechanism
- Clear token on logout

**Server-Side:**
- Never log full token content
- Validate token on every request
- Implement token blacklist for logout (if needed)

### HTTPS Requirements

**Production:**
- All API endpoints must use HTTPS
- HTTP requests redirected to HTTPS
- SSL/TLS certificate required
- HSTS headers configured

**Development:**
- HTTPS recommended
- Self-signed certificates acceptable
- HTTP allowed for local testing only

### Input Validation

**FluentValidation:**
- All command/query requests validated
- Validation rules defined in validators
- Invalid requests rejected with 400 Bad Request
- Detailed error messages for validation failures

**Entity Validation:**
- Domain entities enforce business rules
- Invalid state transitions prevented
- Argument exceptions for invalid inputs

### SQL Injection Prevention

**Entity Framework Core:**
- Parameterized queries used by default
- No raw SQL concatenation
- SQL injection protection built-in

**Dapper:**
- Parameterized queries used
- No string concatenation for SQL
- Safe parameter binding

### CORS Configuration

**Development:**
- `AllowAll` policy configured for local development
- Allows any origin, method, and header

**Production:**
- Restrict to specific origins
- Limit allowed methods
- Limit allowed headers
- Configure credentials if needed

**Example (Production):**
```csharp
options.AddPolicy("ProductionCors", policy =>
{
    policy.WithOrigins("https://app.taskmanagement.com")
        .WithMethods("GET", "POST", "PUT", "DELETE")
        .WithHeaders("Authorization", "Content-Type")
        .AllowCredentials();
});
```

---

## Error Handling Security

### Error Message Sanitization

**Public Error Messages:**
- Business rule violations: Generic messages
- Validation errors: Field-specific messages (safe)
- Not found errors: Generic "Resource not found"
- Authorization errors: Generic "Unauthorized access"

**Private Information:**
- Database errors: Generic "Internal server error"
- Exception details: Not exposed to clients
- Stack traces: Logged only, not returned

### Stack Trace Handling

**Global Exception Handling:**
- `ExceptionHandlingMiddleware` catches unhandled exceptions
- Logs full exception details (server-side only)
- Returns generic error message to client
- Includes trace ID for correlation

**Example Response:**
```json
{
  "success": false,
  "data": null,
  "message": "An error occurred while processing your request",
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### Audit Logging

**Security Events Logged:**
- Authentication attempts (success/failure)
- Authorization failures
- Token validation failures
- Suspicious activity patterns

**Log Format:**
- Structured logging with Serilog
- Includes user ID, timestamp, action, result
- No sensitive information in logs (passwords, tokens)

---

## Configuration Security

### Secret Management

**Development:**
- Secrets in `appsettings.Development.json`
- File excluded from version control (`.gitignore`)
- Local configuration only

**Production:**
- Use environment variables
- Use Azure Key Vault (recommended)
- Use secure configuration providers
- Never commit secrets to version control

**Secrets to Protect:**
- `Jwt:SecretKey`: JWT signing key
- `AzureAd:ClientSecret`: Azure AD client secret
- `ConnectionStrings:DefaultConnection`: Database connection string

### Connection String Security

**Best Practices:**
- Use Integrated Security (Windows Authentication) when possible
- Use Azure Key Vault for connection strings
- Use managed identities in Azure
- Encrypt connection strings at rest
- Rotate credentials regularly

**Example (Azure Key Vault):**
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Azure AD Credentials

**Configuration:**
- Store `ClientSecret` securely (Azure Key Vault)
- Use managed identities when possible
- Rotate client secrets regularly
- Monitor authentication failures

**Token Validation:**
- Validate Azure AD tokens using Microsoft identity libraries
- Verify issuer and audience
- Check token expiration
- Validate signature

---

## Security Checklist

### Authentication
- [ ] Azure AD integration configured correctly
- [ ] JWT token generation working
- [ ] Token validation middleware active
- [ ] Token expiration configured appropriately
- [ ] Invalid token handling tested

### Authorization
- [ ] Role-based access control implemented
- [ ] Endpoint authorization attributes applied
- [ ] Handler-level authorization checks in place
- [ ] Authorization failures return 403 Forbidden
- [ ] Role permissions documented

### Input Validation
- [ ] FluentValidation validators created for all commands/queries
- [ ] Validation errors return appropriate status codes
- [ ] Entity validation enforced
- [ ] SQL injection prevention verified
- [ ] XSS prevention implemented (if applicable)

### Security Configuration
- [ ] HTTPS enabled in production
- [ ] CORS configured appropriately
- [ ] Secrets stored securely
- [ ] Connection strings protected
- [ ] Azure AD credentials secured

### Logging and Monitoring
- [ ] Security events logged
- [ ] Error messages sanitized
- [ ] Stack traces not exposed
- [ ] Audit trail implemented
- [ ] Monitoring alerts configured

---

## See Also

- [Configuration Guide](CONFIGURATION.md) - Configuration options and setup
- [API Reference](API_REFERENCE.md) - Endpoint authorization requirements
- [Architecture Documentation](ARCHITECTURE.md) - System architecture


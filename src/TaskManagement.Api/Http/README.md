# Task Management API - HTTP Request Files

This folder contains HTTP request files for testing all API endpoints. The files are organized by feature and use global variables for base URL and JWT tokens.

## File Structure

- `00-globals.http` - Global variables (base URL, JWT token, headers)
- `01-authentication.http` - Authentication endpoints
- `02-tasks.http` - Task management endpoints
- `03-dashboard.http` - Dashboard statistics endpoints
- `04-users.http` - User search and management endpoints
- `05-database.http` - Database seeding endpoints (Admin only)
- `06-testing.http` - Testing override endpoints and JWT generator (Development/Test only)

## Usage

### 1. Set Global Variables

Open `00-globals.http` and update the variables:

```http
@baseUrl = http://localhost:5050
@jwtToken = your-jwt-token-here
```

### 2. Authenticate First

1. Open `01-authentication.http`
2. Update the `azureAdToken` in the request body
3. Execute the authentication request
4. Copy the JWT token from the response
5. Update `@jwtToken` in `00-globals.http` or run the testing JWT generator (see below)

### 3. Use Authorized Endpoints

All other endpoints require the JWT token. The token is automatically included via the `@authHeader` variable:

```http
Authorization: {{authHeader}}
```

### 4. Generate JWT Token (Testing Only)

1. Each `.http` file starts with a **Generate JWT Token by Email** request (`# @name generateJwt`)
2. Run that request once; it returns a JWT token in `data.token`
3. All requests in that file derive their `Authorization` header from `generateJwt.response.body.data.token`
4. If the token expires, rerun the generate request and execute the desired endpoints again

## Features

### Authentication

- Authenticate with Azure AD token
- Returns JWT token for subsequent requests

### Tasks

- **Queries**: Get task by ID, Get tasks list with filters
- **Commands**: Create, Assign, Accept, Reject, Update Progress, Request Info, Reassign, Request Extension, Approve Extension, Mark Completed, Review Completed

### Dashboard

- Get dashboard statistics for current user

### Users

- Search managed users (manager-employee relationship)
- Get user by ID
- Get current user information

### Database

- Seed database with SQL scripts
- Get available seeding scripts
- **Requires**: Admin role

### Testing

- Set/Get/Remove current user override
- Set/Get/Remove current date override
- Generate JWT tokens by email (updates global `jwtToken` variable automatically)
- **Only available in**: Development and Test environments

## Authorization Roles

- **Employee**: Can accept/reject tasks, update progress, request info, request extensions
- **Manager**: All Employee permissions + assign tasks, reassign tasks, approve extensions, mark completed, review completed tasks
- **Admin**: All Manager permissions + database operations

## Notes

- All endpoints use the standardized `ApiResponse<T>` format
- Successful responses include HATEOAS links for available actions
- Error responses include detailed error information with trace IDs
- Replace placeholder GUIDs (`00000000-0000-0000-0000-000000000000`) with actual IDs when testing

## IDE Support

These `.http` files work with:

- **Visual Studio Code**: REST Client extension
- **JetBrains Rider**: Built-in HTTP Client
- **Visual Studio**: REST Client extension

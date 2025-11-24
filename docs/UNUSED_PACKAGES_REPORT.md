# Unused Package References Report

**Date:** December 2025  
**Status:** Analysis Complete

## Summary

After analyzing all `.csproj` files and their actual usage in the codebase, here are the findings:

## ✅ All Packages Are Used (No Unused Packages Found)

All package references in the project files are either:
1. **Directly used** in the code
2. **Transitively required** by other packages
3. **Design-time tools** (like EF Core Design for migrations)

## Detailed Analysis by Project

### 1. TaskManagement.Application

**Packages:**
- ✅ `FluentValidation.DependencyInjectionExtensions` - Used in `DependencyInjection.cs` (`AddValidatorsFromAssembly`)
- ✅ `Microsoft.AspNetCore.Http.Abstractions` - Used for `IHttpContextAccessor` in `CurrentUserService`
- ✅ `Microsoft.AspNetCore.Http` - Used for `HttpContext` in `CurrentUserService`
- ✅ `Microsoft.Extensions.Configuration.Binder` - **Potentially unused directly**, but `services.Configure<T>()` from `Microsoft.Extensions.Options.ConfigurationExtensions` may use it internally. However, since `configuration.Get<T>()` is not used in Application layer, this could be removed if not needed by `Configure<T>()`.
- ✅ `Microsoft.Extensions.Options.ConfigurationExtensions` - Used for `services.Configure<T>()` in `DependencyInjection.cs`
- ✅ `Microsoft.Extensions.Caching.Memory` - Used for `IMemoryCache` in `CurrentUserService` and `CurrentDateService`

**Recommendation:** `Microsoft.Extensions.Configuration.Binder` in Application layer might be removable if `Configure<T>()` doesn't require it. However, it's safer to keep it as it's a common dependency.

### 2. TaskManagement.Infrastructure

**Packages:**
- ✅ `Microsoft.EntityFrameworkCore` - Used for `DbContext` and EF Core operations
- ✅ `Microsoft.EntityFrameworkCore.SqlServer` - Used for SQL Server provider
- ✅ `Microsoft.EntityFrameworkCore.Design` - **Design-time tool** - Required for EF Core migrations (`dotnet ef migrations`)
- ✅ `Dapper` - Used in all Dapper repository implementations
- ✅ `Microsoft.IdentityModel.Protocols.OpenIdConnect` - Used in `AuthenticationService.cs` for Azure AD configuration

**All packages are used.**

### 3. TaskManagement.Presentation

**Packages:**
- ✅ `Microsoft.OpenApi` - Used in `DependencyInjection.cs` for `OpenApiInfo`, `OpenApiSecurityScheme`, etc.
- ✅ `OpenTelemetry.Instrumentation.Process` - Used in `DependencyInjection.cs` (`AddProcessInstrumentation()`)
- ✅ All other OpenTelemetry packages - Used in observability configuration
- ✅ `Swashbuckle.AspNetCore` - Used for Swagger/OpenAPI generation
- ✅ All authentication packages - Used for Azure AD and JWT authentication

**All packages are used.**

### 4. TaskManagement.Api

**Packages:**
- ✅ All packages are duplicates of Presentation layer packages (since Api references Presentation)
- ✅ `Swashbuckle.AspNetCore` - Used in `Program.cs` (`UseSwagger()`, `UseSwaggerUI()`)
- ✅ All OpenTelemetry packages - Used via `builder.AddObservability()` extension method

**Note:** The Api project references Presentation, so many packages are transitively available. However, some are explicitly referenced for direct usage in `Program.cs`.

### 5. TaskManagement.Tests

**Packages:**
- ✅ `Microsoft.NET.Test.Sdk` - Required for test execution
- ✅ `Microsoft.AspNetCore.Mvc.Testing` - Used for integration tests
- ✅ `Microsoft.EntityFrameworkCore.InMemory` - Used in `InMemoryDatabaseTestBase`
- ⚠️ `Microsoft.OpenApi` - **Potentially unused** - Not found in test code
- ✅ `Moq` - Used for mocking in tests
- ✅ `FluentAssertions` - Used for test assertions
- ✅ `xunit` - Test framework
- ✅ `xunit.runner.visualstudio` - Test runner
- ✅ `coverlet.collector` - Code coverage collection

**Recommendation:** Remove `Microsoft.OpenApi` from Tests project if not used.

### 6. Directory.Packages.props (Central Package Management)

**Potentially Unused Packages (not referenced in any .csproj):**
- ⚠️ `Quartz.AspNetCore` - Not found in any project file
- ⚠️ `Refit.HttpClientFactory` - Not found in any project file
- ⚠️ `Refit.Newtonsoft.Json` - Not found in any project file
- ⚠️ `Scalar.AspNetCore` - Not found in any project file

**Recommendation:** These packages are defined in `Directory.Packages.props` but not referenced in any `.csproj` file. They can be removed from the central package management file.

## Recommendations

### High Priority (Safe to Remove)

1. **Remove from `TaskManagement.Tests.csproj`:**
   ```xml
   <PackageReference Include="Microsoft.OpenApi" />
   ```
   - Not used in test code

2. **Remove from `Directory.Packages.props`:**
   ```xml
   <PackageVersion Include="Quartz.AspNetCore" Version="3.15.0" />
   <PackageVersion Include="Refit.HttpClientFactory" Version="8.0.0" />
   <PackageVersion Include="Refit.Newtonsoft.Json" Version="8.0.0" />
   <PackageVersion Include="Scalar.AspNetCore" Version="2.9.0" />
   ```
   - Not referenced in any project

### Low Priority (Investigate Further)

1. **`Microsoft.Extensions.Configuration.Binder` in Application layer:**
   - Not directly used (no `configuration.Get<T>()` calls)
   - May be required by `services.Configure<T>()` internally
   - **Recommendation:** Keep it (safe, small package)

## Verification Steps

To verify these findings:

1. **Remove `Microsoft.OpenApi` from Tests:**
   ```bash
   dotnet remove tests/TaskManagement.Tests/TaskManagement.Tests.csproj package Microsoft.OpenApi
   dotnet build
   ```

2. **Remove unused packages from Directory.Packages.props:**
   - Remove the 4 packages listed above
   - Run `dotnet restore` and `dotnet build` to verify

3. **Test the application:**
   - Run all tests: `dotnet test`
   - Run the API: `dotnet run --project src/TaskManagement.Api`
   - Verify Swagger UI works
   - Verify OpenTelemetry works

## Conclusion

**Total Unused Packages Found:** 5
- 1 in Tests project (`Microsoft.OpenApi`)
- 4 in Directory.Packages.props (not referenced anywhere)

All other packages are actively used in the codebase.

---

**Last Updated:** December 2025






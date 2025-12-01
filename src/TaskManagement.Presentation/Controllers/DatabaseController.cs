using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DatabaseSeeding.Commands.SeedDatabase;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using static TaskManagement.Domain.Constants.RoleNames;

namespace TaskManagement.Presentation.Controllers;

/// <summary>
///     Controller for database management operations.
/// </summary>
[ApiController]
[Route("database")]
[Authorize(Roles = Admin)]
public class DatabaseController(
    ICommandMediator commandMediator,
    IRequestMediator requestMediator,
    ICurrentUserService currentUserService,
    ILocalizationService localizationService)
    : BaseController(commandMediator, requestMediator, currentUserService, localizationService)
{
    /// <summary>
    ///     Seeds the database with initial data from SQL script files in the scripts/Seeding folder.
    ///     Scripts are executed in alphabetical order by filename.
    /// </summary>
    /// <param name="request">Optional: List of specific script names to execute (if empty, executes all scripts).</param>
    /// <returns>Seeding operation results with execution details for each script.</returns>
    [HttpPost("seed")]
    [SwaggerOperation(
        Summary = "Seed Database",
        Description =
            "Seeds the database with initial data from SQL script files located in the scripts/Seeding folder. Scripts are executed in alphabetical order by filename. Can execute all scripts or specific scripts by providing script names in the request. Only Admins can execute this operation. Returns detailed execution results for each script including success status, execution time, and any errors."
    )]
    [ProducesResponseType(typeof(ApiResponse<SeedDatabaseResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SeedDatabase([FromBody] SeedDatabaseRequest? request)
    {
        var command = new SeedDatabaseCommand
        {
            ScriptNames = request?.ScriptNames
        };

        var result = await _commandMediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    ///     Gets information about available seeding scripts.
    /// </summary>
    /// <returns>List of available SQL scripts in the seeding folder.</returns>
    [HttpGet("seed/scripts")]
    [SwaggerOperation(
        Summary = "Get Available Seeding Scripts",
        Description =
            "Retrieves a list of all available SQL seeding scripts in the scripts/Seeding folder. Scripts are returned in alphabetical order. Only Admins can access this endpoint. Useful for discovering which scripts are available before executing the seed operation."
    )]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public IActionResult GetAvailableScripts()
    {
        try
        {
            var projectRoot = GetProjectRootDirectory();
            var seedingFolderPath = Path.Combine(projectRoot, "scripts", "Seeding");

            if (!Directory.Exists(seedingFolderPath))
                return NotFound(ApiResponse<object>.ErrorResponse(
                    $"Seeding folder not found at: {seedingFolderPath}",
                    HttpContext.TraceIdentifier));

            var scripts = Directory.GetFiles(seedingFolderPath, "*.sql")
                .Select(Path.GetFileName)
                .Where(name => name != null)
                .Cast<string>()
                .OrderBy(name => name)
                .ToList();

            return Ok(ApiResponse<List<string>>.SuccessResponse(scripts));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                $"Error retrieving scripts: {ex.Message}",
                HttpContext.TraceIdentifier));
        }
    }

    private string GetProjectRootDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(currentDirectory);

        while (directory != null)
        {
            if (directory.GetFiles("*.sln").Any()) return directory.FullName;
            directory = directory.Parent;
        }

        return currentDirectory;
    }
}

/// <summary>
///     Request model for database seeding operation.
/// </summary>
public class SeedDatabaseRequest
{
    /// <summary>
    ///     Optional: Specific script names to execute (if empty or null, executes all scripts).
    /// </summary>
    /// <example>["00-SeedEmployees.sql", "01-Add Employees.sql"]</example>
    public List<string>? ScriptNames { get; set; }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DatabaseSeeding.Commands.SeedDatabase;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.DTOs;
using static TaskManagement.Domain.Constants.RoleNames;

namespace TaskManagement.Presentation.Controllers;

/// <summary>
///     Controller for database management operations.
/// </summary>
[ApiController]
[Route("database")]
[Authorize(Roles = Admin)]
public class DatabaseController(ICommandMediator commandMediator, IRequestMediator requestMediator)
    : BaseController(commandMediator, requestMediator)
{

    /// <summary>
    ///     Seeds the database with initial data from SQL script files in the scripts/Seeding folder.
    ///     Scripts are executed in alphabetical order by filename.
    /// </summary>
    /// <param name="request">Optional: List of specific script names to execute (if empty, executes all scripts).</param>
    /// <returns>Seeding operation results with execution details for each script.</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /database/seed
    ///     {
    ///       "scriptNames": ["00-SeedEmployees.sql"]
    ///     }
    ///     
    /// To execute all scripts, send an empty request body or omit scriptNames:
    /// 
    ///     POST /database/seed
    ///     {}
    /// 
    /// </remarks>
    /// <response code="200">Database seeded successfully with execution details</response>
    /// <response code="400">Invalid request or seeding failed</response>
    /// <response code="401">Unauthorized - missing or invalid authentication token</response>
    /// <response code="403">Forbidden - user does not have Admin role</response>
    [HttpPost("seed")]
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
    /// <response code="200">List of available scripts</response>
    /// <response code="401">Unauthorized - missing or invalid authentication token</response>
    /// <response code="403">Forbidden - user does not have Admin role</response>
    [HttpGet("seed/scripts")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAvailableScripts()
    {
        try
        {
            var projectRoot = GetProjectRootDirectory();
            var seedingFolderPath = Path.Combine(projectRoot, "scripts", "Seeding");

            if (!Directory.Exists(seedingFolderPath))
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    $"Seeding folder not found at: {seedingFolderPath}",
                    HttpContext.TraceIdentifier));
            }

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
            if (directory.GetFiles("*.sln").Any())
            {
                return directory.FullName;
            }
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


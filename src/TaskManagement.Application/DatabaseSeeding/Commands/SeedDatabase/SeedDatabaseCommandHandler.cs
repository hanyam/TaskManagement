using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.DatabaseSeeding.Commands.SeedDatabase;

/// <summary>
///     Handler for seeding the database with SQL scripts from the seeding folder.
/// </summary>
public class SeedDatabaseCommandHandler(
    TaskManagementDbContext context,
    ILogger<SeedDatabaseCommandHandler> logger) : ICommandHandler<SeedDatabaseCommand, SeedDatabaseResultDto>
{
    private const string SeedingFolderPath = "scripts/Seeding";
    private readonly TaskManagementDbContext _context = context;
    private readonly ILogger<SeedDatabaseCommandHandler> _logger = logger;

    public async Task<Result<SeedDatabaseResultDto>> Handle(
        SeedDatabaseCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting database seeding operation");

        var overallStopwatch = Stopwatch.StartNew();
        var result = new SeedDatabaseResultDto();

        try
        {
            // Get the project root directory (where the solution file is)
            var projectRoot = GetProjectRootDirectory();
            var seedingFolderFullPath = Path.Combine(projectRoot, SeedingFolderPath);

            _logger.LogInformation("Looking for seeding scripts in: {Path}", seedingFolderFullPath);

            if (!Directory.Exists(seedingFolderFullPath))
            {
                _logger.LogError("Seeding folder not found: {Path}", seedingFolderFullPath);
                return Result<SeedDatabaseResultDto>.Failure(
                    Error.NotFound($"Seeding folder not found at path: {seedingFolderFullPath}"));
            }

            // Get all SQL files and order them by name
            var sqlFiles = Directory.GetFiles(seedingFolderFullPath, "*.sql")
                .OrderBy(f => Path.GetFileName(f))
                .ToList();

            // Filter by specific script names if provided
            if (request.ScriptNames?.Any() == true)
                sqlFiles = sqlFiles
                    .Where(f => request.ScriptNames.Contains(Path.GetFileName(f)))
                    .ToList();

            result.TotalScripts = sqlFiles.Count;

            if (sqlFiles.Count == 0)
            {
                _logger.LogWarning("No SQL files found in seeding folder");
                return Result<SeedDatabaseResultDto>.Success(result);
            }

            _logger.LogInformation("Found {Count} SQL files to execute", sqlFiles.Count);

            // Execute each script
            foreach (var scriptPath in sqlFiles)
            {
                var scriptName = Path.GetFileName(scriptPath);
                var executionDetail = await ExecuteScript(scriptPath, scriptName, cancellationToken);
                result.ExecutionDetails.Add(executionDetail);

                if (executionDetail.Success)
                {
                    result.SuccessfulScripts++;
                }
                else
                {
                    result.FailedScripts++;
                    _logger.LogError(
                        "Script {ScriptName} failed: {Error}",
                        scriptName,
                        executionDetail.ErrorMessage);
                }
            }

            overallStopwatch.Stop();
            result.TotalExecutionTimeMs = overallStopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Database seeding completed. Total: {Total}, Success: {Success}, Failed: {Failed}, Time: {Time}ms",
                result.TotalScripts,
                result.SuccessfulScripts,
                result.FailedScripts,
                result.TotalExecutionTimeMs);

            return Result<SeedDatabaseResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database seeding operation");
            return Result<SeedDatabaseResultDto>.Failure(
                Error.Internal($"Database seeding failed: {ex.Message}"));
        }
    }

    private async Task<ScriptExecutionDetail> ExecuteScript(
        string scriptPath,
        string scriptName,
        CancellationToken cancellationToken)
    {
        var detail = new ScriptExecutionDetail
        {
            ScriptName = scriptName
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Executing script: {ScriptName}", scriptName);

            // Read the SQL script
            var sqlScript = await File.ReadAllTextAsync(scriptPath, cancellationToken);

            if (string.IsNullOrWhiteSpace(sqlScript)) throw new InvalidOperationException("Script file is empty");

            // Execute the script using raw SQL
            // Split by GO statements (if any) and execute each batch
            var batches = SplitSqlBatches(sqlScript);

            var totalRowsAffected = 0;

            foreach (var batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch))
                    continue;

                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(batch, cancellationToken);
                totalRowsAffected += rowsAffected;
            }

            stopwatch.Stop();

            detail.Success = true;
            detail.RowsAffected = totalRowsAffected;
            detail.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Script {ScriptName} executed successfully in {Time}ms, rows affected: {Rows}",
                scriptName,
                detail.ExecutionTimeMs,
                detail.RowsAffected);
        }
        catch (SqlException ex)
        {
            stopwatch.Stop();
            detail.Success = false;
            detail.ErrorMessage = $"SQL Error: {ex.Message} (Line: {ex.LineNumber})";
            detail.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogError(
                ex,
                "SQL error executing script {ScriptName}: {Message}",
                scriptName,
                ex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            detail.Success = false;
            detail.ErrorMessage = ex.Message;
            detail.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogError(
                ex,
                "Error executing script {ScriptName}: {Message}",
                scriptName,
                ex.Message);
        }

        return detail;
    }

    /// <summary>
    ///     Splits SQL script by GO statements (SQL Server batch separator).
    /// </summary>
    private List<string> SplitSqlBatches(string sqlScript)
    {
        // Split by GO statements (case insensitive, must be on its own line)
        var batches = new List<string>();
        var lines = sqlScript.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var currentBatch = new List<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Check if line is a GO statement
            if (trimmedLine.Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                // Add current batch if not empty
                if (currentBatch.Any())
                {
                    batches.Add(string.Join(Environment.NewLine, currentBatch));
                    currentBatch.Clear();
                }
            }
            else
            {
                currentBatch.Add(line);
            }
        }

        // Add remaining batch
        if (currentBatch.Any()) batches.Add(string.Join(Environment.NewLine, currentBatch));

        return batches;
    }

    /// <summary>
    ///     Gets the project root directory by searching for the solution file.
    /// </summary>
    private string GetProjectRootDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        // Navigate up until we find the solution file
        var directory = new DirectoryInfo(currentDirectory);

        while (directory != null)
        {
            // Check if this directory contains a .sln file
            if (directory.GetFiles("*.sln").Any()) return directory.FullName;

            directory = directory.Parent;
        }

        // If not found, return current directory as fallback
        _logger.LogWarning(
            "Could not find solution root, using current directory: {Path}",
            currentDirectory);

        return currentDirectory;
    }
}
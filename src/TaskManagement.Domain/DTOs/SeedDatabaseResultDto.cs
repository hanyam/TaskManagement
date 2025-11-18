namespace TaskManagement.Domain.DTOs;

/// <summary>
///     Result of database seeding operation.
/// </summary>
public class SeedDatabaseResultDto
{
    /// <summary>
    ///     Total number of scripts found.
    /// </summary>
    public int TotalScripts { get; set; }

    /// <summary>
    ///     Number of scripts executed successfully.
    /// </summary>
    public int SuccessfulScripts { get; set; }

    /// <summary>
    ///     Number of scripts that failed.
    /// </summary>
    public int FailedScripts { get; set; }

    /// <summary>
    ///     Details of each script execution.
    /// </summary>
    public List<ScriptExecutionDetail> ExecutionDetails { get; set; } = new();

    /// <summary>
    ///     Total execution time in milliseconds.
    /// </summary>
    public long TotalExecutionTimeMs { get; set; }
}

/// <summary>
///     Details of a single script execution.
/// </summary>
public class ScriptExecutionDetail
{
    /// <summary>
    ///     Script file name.
    /// </summary>
    public string ScriptName { get; set; } = string.Empty;

    /// <summary>
    ///     Whether the script executed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    ///     Error message if execution failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     Execution time in milliseconds.
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    ///     Number of rows affected (if available).
    /// </summary>
    public int? RowsAffected { get; set; }
}


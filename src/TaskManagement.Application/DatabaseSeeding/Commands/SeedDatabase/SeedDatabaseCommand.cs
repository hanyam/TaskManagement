using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.DatabaseSeeding.Commands.SeedDatabase;

/// <summary>
///     Command to seed the database with initial data from SQL script files.
/// </summary>
public record SeedDatabaseCommand : ICommand<SeedDatabaseResultDto>
{
    /// <summary>
    ///     Optional: Specific script names to execute (if empty, executes all scripts).
    /// </summary>
    public List<string>? ScriptNames { get; init; }
}
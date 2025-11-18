using FluentValidation;

namespace TaskManagement.Application.Tasks.Commands.AssignTask;

/// <summary>
///     Validator for the AssignTaskCommand.
/// </summary>
public class AssignTaskCommandValidator : AbstractValidator<AssignTaskCommand>
{
    public AssignTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");

        RuleFor(x => x.UserIds)
            .NotNull().WithMessage("User IDs are required")
            .Must(userIds => userIds != null && userIds.Any()).WithMessage("At least one user must be assigned")
            .Must(userIds => userIds == null || userIds.All(id => id != Guid.Empty))
            .WithMessage("User ID cannot be empty");

        RuleFor(x => x.AssignedById)
            .NotEmpty().WithMessage("Assigned by user ID is required");
    }
}
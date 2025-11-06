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
            .NotEmpty().WithMessage("At least one user must be assigned")
            .Must(ids => ids?.Count > 0).WithMessage("At least one user ID is required");

        RuleForEach(x => x.UserIds)
            .NotEmpty().WithMessage("User ID cannot be empty");
        
        RuleFor(x => x.AssignedById)
            .NotEmpty().WithMessage("Assigned by user ID is required");
    }
}


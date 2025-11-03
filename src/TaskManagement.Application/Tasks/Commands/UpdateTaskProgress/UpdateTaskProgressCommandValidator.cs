using FluentValidation;

namespace TaskManagement.Application.Tasks.Commands.UpdateTaskProgress;

/// <summary>
///     Validator for the UpdateTaskProgressCommand.
/// </summary>
public class UpdateTaskProgressCommandValidator : AbstractValidator<UpdateTaskProgressCommand>
{
    public UpdateTaskProgressCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");

        RuleFor(x => x.ProgressPercentage)
            .InclusiveBetween(0, 100).WithMessage("Progress percentage must be between 0 and 100");

        RuleFor(x => x.UpdatedById)
            .NotEmpty().WithMessage("Updated by user ID is required");
    }
}


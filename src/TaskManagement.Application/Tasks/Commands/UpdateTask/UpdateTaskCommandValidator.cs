using FluentValidation;
using TaskManagement.Application.Tasks.Commands.UpdateTask;

namespace TaskManagement.Application.Tasks.Commands.UpdateTask;

/// <summary>
///     Validator for the UpdateTaskCommand.
/// </summary>
public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("Task ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Priority must be a valid value");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Due date must be in the future")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.UpdatedById)
            .NotEmpty()
            .WithMessage("Updated by user ID is required");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty()
            .WithMessage("Updated by user email is required");
    }
}


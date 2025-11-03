using FluentValidation;

namespace TaskManagement.Application.Tasks.Commands.CreateTask;

/// <summary>
///     Validator for the CreateTaskCommand.
/// </summary>
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.AssignedUserId)
            .NotEmpty().WithMessage("Assigned user ID is required");

        RuleFor(x => x.CreatedById)
            .NotEmpty().WithMessage("Created by user ID is required");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("Created by is required");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future")
            .When(x => x.DueDate.HasValue);
    }
}
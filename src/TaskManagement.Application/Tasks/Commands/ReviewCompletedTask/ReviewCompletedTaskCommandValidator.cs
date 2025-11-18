using FluentValidation;

namespace TaskManagement.Application.Tasks.Commands.ReviewCompletedTask;

/// <summary>
///     Validator for ReviewCompletedTaskCommand.
/// </summary>
public class ReviewCompletedTaskCommandValidator : AbstractValidator<ReviewCompletedTaskCommand>
{
    public ReviewCompletedTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("Task ID is required");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Feedback)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Feedback))
            .WithMessage("Feedback cannot exceed 1000 characters");

        RuleFor(x => x.SendBackForRework)
            .Must((command, sendBack) => !sendBack || !command.Accepted)
            .WithMessage("Cannot accept and send back for rework at the same time");
    }
}
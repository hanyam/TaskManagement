using FluentValidation;

namespace TaskManagement.Application.Tasks.Commands.RequestDeadlineExtension;

/// <summary>
///     Validator for the RequestDeadlineExtensionCommand.
/// </summary>
public class RequestDeadlineExtensionCommandValidator : AbstractValidator<RequestDeadlineExtensionCommand>
{
    public RequestDeadlineExtensionCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");

        RuleFor(x => x.RequestedDueDate)
            .NotEmpty().WithMessage("Requested due date is required")
            .Must(date => date > DateTime.UtcNow).WithMessage("Requested due date must be in the future");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");

        RuleFor(x => x.RequestedById)
            .NotEmpty().WithMessage("Requested by user ID is required");
    }
}
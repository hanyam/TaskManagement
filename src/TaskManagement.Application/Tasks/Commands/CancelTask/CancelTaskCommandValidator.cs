using FluentValidation;

namespace TaskManagement.Application.Tasks.Commands.CancelTask;

/// <summary>
///     Validator for <see cref="CancelTaskCommand"/>.
/// </summary>
public class CancelTaskCommandValidator : AbstractValidator<CancelTaskCommand>
{
    public CancelTaskCommandValidator()
    {
        RuleFor(command => command.TaskId)
            .NotEmpty()
            .WithMessage("TaskId is required.");

        RuleFor(command => command.RequestedById)
            .NotEmpty()
            .WithMessage("RequestedById is required.");

        RuleFor(command => command.RequestedByRole)
            .NotEmpty()
            .WithMessage("RequestedByRole is required.");
    }
}



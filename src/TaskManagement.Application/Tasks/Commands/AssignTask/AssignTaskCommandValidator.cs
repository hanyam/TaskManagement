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

        // RuleFor(x => x.UserIds)
        //     .NotNull().WithMessage("User IDs are required")
        //     .Custom((userIds, context) =>
        //     {
        //         if (userIds == null) return;
        //         
        //         if (!userIds.Any())
        //         {
        //             context.AddFailure("At least one user must be assigned");
        //             return;
        //         }
        //
        //         if (userIds.Any(id => id == Guid.Empty))
        //         {
        //             context.AddFailure("User ID cannot be empty");
        //         }
        //     });
        //
        // RuleFor(x => x.AssignedById)
        //     .NotEmpty().WithMessage("Assigned by user ID is required");
    }
}


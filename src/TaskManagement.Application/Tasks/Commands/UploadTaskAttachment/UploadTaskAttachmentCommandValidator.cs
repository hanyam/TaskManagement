using FluentValidation;
using Microsoft.Extensions.Options;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Domain.Options;

namespace TaskManagement.Application.Tasks.Commands.UploadTaskAttachment;

/// <summary>
///     Validator for UploadTaskAttachmentCommand.
/// </summary>
public class UploadTaskAttachmentCommandValidator : AbstractValidator<UploadTaskAttachmentCommand>
{
    public UploadTaskAttachmentCommandValidator(IOptions<FileStorageOptions> fileStorageOptions)
    {
        var maxFileSize = fileStorageOptions.Value.MaxFileSizeBytes;

        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage(TaskErrors.InvalidTaskId.Message);

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("File stream is required");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage(TaskErrors.InvalidFileName.Message)
            .MaximumLength(500)
            .WithMessage("File name cannot exceed 500 characters");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required")
            .MaximumLength(255)
            .WithMessage("Content type cannot exceed 255 characters");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("File size must be greater than zero")
            .LessThanOrEqualTo(maxFileSize)
            .WithMessage($"File size cannot exceed {maxFileSize / 1024 / 1024}MB");

        RuleFor(x => x.UploadedById)
            .NotEmpty()
            .WithMessage("Uploaded by user ID is required");

        RuleFor(x => x.UploadedBy)
            .NotEmpty()
            .WithMessage("Uploaded by user email is required");
    }
}


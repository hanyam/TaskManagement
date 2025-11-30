using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Application.Tasks.Commands.CancelTask;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Tests.Unit.TestHelpers;
using Xunit;
using TaskStatusEnum = TaskManagement.Domain.Entities.TaskStatus;
using SystemTask = System.Threading.Tasks.Task;

namespace TaskManagement.Tests.Unit.Application.Tasks.Commands.CancelTask;

public class CancelTaskCommandHandlerTests : InMemoryDatabaseTestBase
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly CancelTaskCommandHandler _handler;

    public CancelTaskCommandHandlerTests()
    {
        var repository = new TaskEfCommandRepository(Context);
        var logger = Mock.Of<ILogger<CancelTaskCommandHandler>>();
        var taskHistoryService = new Mock<ITaskHistoryService>().Object;
        _handler = new CancelTaskCommandHandler(Context, repository, _fileStorageServiceMock.Object, logger, taskHistoryService);
    }

    [Fact]
    public async SystemTask Handle_PreAcceptanceTask_ShouldDeleteTaskAndAttachments()
    {
        var manager = GetTestUserWithRole("john.doe@example.com", UserRole.Manager);
        var task = CreateTestTask(
            "Draft Task",
            "To be deleted",
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(2),
            manager.Id,
            TaskType.Simple,
            manager.Id);

        var attachment = new TaskAttachment(
            task.Id,
            "spec.pdf",
            "spec.pdf",
            "application/pdf",
            1024,
            "attachments/spec.pdf",
            AttachmentType.ManagerUploaded,
            manager.Id);
        attachment.SetCreatedBy("test");
        Context.TaskAttachments.Add(attachment);
        Context.SaveChanges();

        var command = new CancelTaskCommand
        {
            TaskId = task.Id,
            RequestedById = manager.Id,
            RequestedByRole = RoleNames.Manager
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await Context.Tasks.FindAsync(task.Id)).Should().BeNull();
        _fileStorageServiceMock.Verify(
            s => s.DeleteFileAsync(attachment.StoragePath, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async SystemTask Handle_AfterAcceptance_ShouldSetStatusToCancelled()
    {
        var manager = GetTestUserWithRole("john.doe@example.com", UserRole.Manager);
        var employee = GetTestUser("jane.smith@example.com");
        var task = CreateTestTask(
            "Accepted Task",
            "Needs to be cancelled",
            TaskPriority.High,
            DateTime.UtcNow.AddDays(5),
            employee.Id,
            TaskType.WithProgress,
            manager.Id);

        task.Assign();
        task.Accept();
        Context.SaveChanges();

        var command = new CancelTaskCommand
        {
            TaskId = task.Id,
            RequestedById = manager.Id,
            RequestedByRole = RoleNames.Manager
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(TaskStatusEnum.Cancelled);
    }

    [Fact]
    public async SystemTask Handle_ReviewedTask_ShouldReturnFailure()
    {
        var manager = GetTestUserWithRole("john.doe@example.com", UserRole.Manager);
        var employee = GetTestUser("jane.smith@example.com");
        var task = CreateTestTask(
            "Reviewed Task",
            "Already reviewed",
            TaskPriority.High,
            DateTime.UtcNow.AddDays(5),
            employee.Id,
            TaskType.WithProgress,
            manager.Id);

        task.Assign();
        task.Accept();
        task.MarkCompletedByEmployee();
        task.ReviewByManager(true, 5, "Great work");
        Context.SaveChanges();

        var command = new CancelTaskCommand
        {
            TaskId = task.Id,
            RequestedById = manager.Id,
            RequestedByRole = RoleNames.Manager
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ShouldContainError(TaskErrors.CannotCancelReviewedTask);
    }
}



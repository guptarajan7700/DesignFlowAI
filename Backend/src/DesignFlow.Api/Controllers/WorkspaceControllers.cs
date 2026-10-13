using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Contracts;
using DesignFlow.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DesignFlow.Api.Controllers;

/// <summary>People directory for assignment menus.</summary>
[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IWorkspaceService _workspace;

    public UsersController(IWorkspaceService workspace) => _workspace = workspace;

    /// <summary>Lists active studio users and their roles.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TeamMemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeamMemberDto>>> List(CancellationToken cancellationToken) =>
        Ok(await _workspace.ListUsersAsync(cancellationToken));
}

/// <summary>In-app alerts for the signed-in person.</summary>
[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly IWorkspaceService _workspace;
    private readonly ICurrentUser _currentUser;

    public NotificationsController(IWorkspaceService workspace, ICurrentUser currentUser)
    {
        _workspace = workspace;
        _currentUser = currentUser;
    }

    /// <summary>Returns the newest notifications for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> List(CancellationToken cancellationToken) =>
        Ok(await _workspace.ListNotificationsAsync(_currentUser.UserId, cancellationToken));

    /// <summary>Marks one notification as read.</summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await _workspace.MarkNotificationReadAsync(id, _currentUser.UserId, cancellationToken);
        return NoContent();
    }
}

/// <summary>Task board moves and manual tasks.</summary>
[ApiController]
[Authorize(Roles = RoleNames.Staff)]
public sealed class TasksController : ControllerBase
{
    private readonly IWorkspaceService _workspace;
    private readonly ICurrentUser _currentUser;

    public TasksController(IWorkspaceService workspace, ICurrentUser currentUser)
    {
        _workspace = workspace;
        _currentUser = currentUser;
    }

    /// <summary>Adds a manual task in To Do. AI proposals still go through the plan approval flow.</summary>
    [HttpPost("/api/projects/{projectId:guid}/tasks")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> Create(Guid projectId, CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await _workspace.CreateTaskAsync(projectId, request, _currentUser.UserId, cancellationToken);
        return Created($"/api/projects/{projectId}", task);
    }

    /// <summary>Moves an approved task across the board.</summary>
    [HttpPatch("/api/tasks/{id:guid}/status")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> UpdateStatus(Guid id, UpdateTaskStatusRequest request, CancellationToken cancellationToken) =>
        Ok(await _workspace.UpdateTaskStatusAsync(id, request.Status, cancellationToken));
}

/// <summary>Recent automation jobs for the studio queue.</summary>
[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/automation/jobs")]
public sealed class AutomationJobsQueryController : ControllerBase
{
    private readonly IWorkspaceService _workspace;

    public AutomationJobsQueryController(IWorkspaceService workspace) => _workspace = workspace;

    /// <summary>Lists the latest automation jobs.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AutomationJobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AutomationJobDto>>> List(CancellationToken cancellationToken) =>
        Ok(await _workspace.ListJobsAsync(cancellationToken));
}

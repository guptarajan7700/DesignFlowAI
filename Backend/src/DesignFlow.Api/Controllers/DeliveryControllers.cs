using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Contracts;
using DesignFlow.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DesignFlow.Api.Controllers;

/// <summary>Validation issues raised from model checks.</summary>
[ApiController]
[Authorize]
[Route("api/issues")]
public sealed class IssuesController : ControllerBase
{
    private readonly IIssueService _issues;
    private readonly ICurrentUser _currentUser;

    public IssuesController(IIssueService issues, ICurrentUser currentUser)
    {
        _issues = issues;
        _currentUser = currentUser;
    }

    /// <summary>Lists issues. Filter by project, severity, or status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<IssueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<IssueDto>>> List(
        [FromQuery] Guid? projectId,
        [FromQuery] string? severity,
        [FromQuery] string? status,
        CancellationToken cancellationToken) =>
        Ok(await _issues.ListAsync(projectId, severity, status, _currentUser.UserId, _currentUser.Roles, cancellationToken));

    /// <summary>Assigns an issue to a designer and notifies them.</summary>
    [Authorize(Roles = RoleNames.Managers)]
    [HttpPost("{id:guid}/assign")]
    [ProducesResponseType(typeof(IssueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IssueDto>> Assign(Guid id, AssignIssueRequest request, CancellationToken cancellationToken) =>
        Ok(await _issues.AssignAsync(id, request.AssigneeId, _currentUser.UserId, cancellationToken));
}

/// <summary>Revit model sync and validation runs posted by the desktop add-in.</summary>
[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/revit")]
public sealed class RevitController : ControllerBase
{
    private readonly IRevitService _revit;
    private readonly ICurrentUser _currentUser;

    public RevitController(IRevitService revit, ICurrentUser currentUser)
    {
        _revit = revit;
        _currentUser = currentUser;
    }

    /// <summary>Creates or updates a Revit model record after a desktop sync.</summary>
    [HttpPost("models/sync")]
    [ProducesResponseType(typeof(RevitModelDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RevitModelDto>> Sync(SyncModelRequest request, CancellationToken cancellationToken) =>
        Ok(await _revit.SyncAsync(request, _currentUser.UserId, cancellationToken));

    /// <summary>
    /// Stores a validation run. A failed run opens issues, assigns the lead designer,
    /// sets the project to NeedsAttention, and creates a notification.
    /// </summary>
    [HttpPost("validation-runs")]
    [ProducesResponseType(typeof(ValidationRunDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ValidationRunDto>> CreateRun(CreateValidationRunRequest request, CancellationToken cancellationToken)
    {
        var run = await _revit.CreateValidationRunAsync(request, _currentUser.UserId, cancellationToken);
        return Created($"/api/revit/validation-runs/{run.ValidationRunId}", run);
    }
}

/// <summary>Background automation queue.</summary>
[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/automation/jobs")]
public sealed class AutomationController : ControllerBase
{
    private readonly IAutomationService _automation;
    private readonly ICurrentUser _currentUser;

    public AutomationController(IAutomationService automation, ICurrentUser currentUser)
    {
        _automation = automation;
        _currentUser = currentUser;
    }

    /// <summary>Queues a job. A background worker moves it from Queued to a terminal status.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AutomationJobDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AutomationJobDto>> Create(CreateAutomationJobRequest request, CancellationToken cancellationToken)
    {
        var job = await _automation.EnqueueAsync(request, _currentUser.UserId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = job.AutomationJobId }, job);
    }

    /// <summary>Returns the current status of one automation job.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AutomationJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AutomationJobDto>> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await _automation.GetAsync(id, cancellationToken));
}

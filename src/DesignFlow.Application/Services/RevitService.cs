using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Common;
using DesignFlow.Application.Contracts;
using DesignFlow.Application.Workflows;
using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;

namespace DesignFlow.Application.Services;

public sealed class RevitService : IRevitService
{
    private readonly IRevitRepository _revit;
    private readonly IProjectRepository _projects;
    private readonly IIssueRepository _issues;
    private readonly INotificationRepository _notifications;
    private readonly IAuditRepository _audit;
    private readonly IUnitOfWork _unitOfWork;

    public RevitService(
        IRevitRepository revit,
        IProjectRepository projects,
        IIssueRepository issues,
        INotificationRepository notifications,
        IAuditRepository audit,
        IUnitOfWork unitOfWork)
    {
        _revit = revit;
        _projects = projects;
        _issues = issues;
        _notifications = notifications;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    public async Task<RevitModelDto> SyncAsync(SyncModelRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        var project = await _projects.GetTrackedAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");

        var fileName = request.FileName.Trim();
        var model = await _revit.FindByProjectAndFileAsync(project.ProjectId, fileName, cancellationToken);
        if (model is null)
        {
            model = new RevitModel
            {
                RevitModelId = Guid.NewGuid(),
                ProjectId = project.ProjectId,
                FileName = fileName,
                CreatedBy = actorId
            };
            await _revit.AddModelAsync(model, cancellationToken);
        }

        model.Name = request.Name.Trim();
        model.VersionLabel = request.VersionLabel?.Trim();
        model.AutodeskUrn = string.IsNullOrWhiteSpace(request.AutodeskUrn) ? model.AutodeskUrn : request.AutodeskUrn.Trim();
        model.AutodeskProjectId = request.AutodeskProjectId?.Trim();
        model.AutodeskItemId = request.AutodeskItemId?.Trim();
        model.LastSyncedAt = DateTime.UtcNow;
        model.Status = RevitModelStatus.Synced;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ProjectMapper.ToModel(model);
    }

    public async Task<ValidationRunDto> CreateValidationRunAsync(
        CreateValidationRunRequest request,
        Guid actorId,
        CancellationToken cancellationToken)
    {
        if (request.Results is null || request.Results.Count == 0)
        {
            throw new AppValidationException("A validation run needs at least one check result.");
        }

        var model = await _revit.GetTrackedModelAsync(request.RevitModelId, cancellationToken)
            ?? throw new NotFoundException("Revit model was not found.");

        var parsed = request.Results.Select((check, index) => new ValidationResult
        {
            ValidationResultId = Guid.NewGuid(),
            CheckCode = check.CheckCode.Trim(),
            CheckName = check.CheckName.Trim(),
            Status = EnumParser.Parse<ValidationResultStatus>(check.Status, "Status"),
            ElementId = check.ElementId?.Trim(),
            ElementCategory = check.ElementCategory?.Trim(),
            Message = check.Message.Trim(),
            SuggestedFix = check.SuggestedFix?.Trim(),
            SortOrder = index + 1,
            CreatedBy = actorId
        }).ToList();

        var failed = parsed.Where(result => result.Status == ValidationResultStatus.Fail).ToList();
        var passed = parsed.Count(result => result.Status == ValidationResultStatus.Pass);
        var utcNow = DateTime.UtcNow;
        var run = new ValidationRun
        {
            ValidationRunId = Guid.NewGuid(),
            RevitModelId = model.RevitModelId,
            ProjectId = model.ProjectId,
            TriggeredById = actorId,
            Status = failed.Count > 0 ? ValidationRunStatus.Failed : ValidationRunStatus.Passed,
            StartedAt = utcNow,
            CompletedAt = utcNow,
            ChecksPassed = passed,
            ChecksFailed = failed.Count,
            Summary = failed.Count == 0
                ? "All submitted checks passed."
                : string.Join(", ", failed.Select(result => result.CheckName)),
            CreatedBy = actorId
        };

        foreach (var result in parsed)
        {
            result.ValidationRunId = run.ValidationRunId;
        }

        await _revit.AddRunAsync(run, cancellationToken);
        await _revit.AddResultsAsync(parsed, cancellationToken);

        var issuesCreated = 0;
        if (run.Status == ValidationRunStatus.Failed)
        {
            var existing = await _issues.ExistingResultIdsAsync(failed.Select(result => result.ValidationResultId), cancellationToken);
            var newFailures = failed.Where(result => !existing.Contains(result.ValidationResultId)).ToList();
            var outcome = ValidationFailureWorkflow.Apply(model.Project, run, newFailures, actorId, utcNow);
            if (outcome.Issues.Count > 0)
            {
                await _issues.AddRangeAsync(outcome.Issues, cancellationToken);
            }

            issuesCreated = outcome.Issues.Count;
            if (outcome.Notification is not null
                && !await _notifications.ExistsAsync(outcome.Notification.Type.ToString(), run.ValidationRunId, cancellationToken))
            {
                await _notifications.AddAsync(outcome.Notification, cancellationToken);
            }

            if (!await _audit.ExistsAsync("ValidationRun", run.ValidationRunId, "CreateIssuesFromFailure", cancellationToken))
            {
                await _audit.AddAsync(outcome.Audit, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new ValidationRunDto(
            run.ValidationRunId,
            run.ProjectId,
            run.RevitModelId,
            run.Status.ToString(),
            run.ChecksPassed,
            run.ChecksFailed,
            run.Summary,
            issuesCreated);
    }
}

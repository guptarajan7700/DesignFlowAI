using System.Text.Json;
using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Ai;
using DesignFlow.Application.Common;
using DesignFlow.Application.Contracts;
using DesignFlow.Domain;
using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;

namespace DesignFlow.Application.Services;

public sealed class AssistantService : IAssistantService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAiService _ai;
    private readonly IProjectRepository _projects;
    private readonly IAiCommandRepository _commands;
    private readonly IRevitRepository _revit;
    private readonly INotificationRepository _notifications;
    private readonly IBusinessReadStore _business;
    private readonly IUnitOfWork _unitOfWork;

    public AssistantService(
        IAiService ai,
        IProjectRepository projects,
        IAiCommandRepository commands,
        IRevitRepository revit,
        INotificationRepository notifications,
        IBusinessReadStore business,
        IUnitOfWork unitOfWork)
    {
        _ai = ai;
        _projects = projects;
        _commands = commands;
        _revit = revit;
        _notifications = notifications;
        _business = business;
        _unitOfWork = unitOfWork;
    }

    public async Task<AnalyzeBriefResponse> AnalyzeAsync(AnalyzeBriefRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        var analysis = await _ai.AnalyzeBriefAsync(request.Brief, cancellationToken);
        if (request.ProjectId is not Guid projectId)
        {
            return new AnalyzeBriefResponse(analysis, false, Array.Empty<RequirementDto>());
        }

        var project = await _projects.GetTrackedAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");
        var sort = await _projects.NextRequirementSortOrderAsync(project.ProjectId, cancellationToken);
        var saved = new List<ProjectRequirement>();
        var analysisJson = JsonSerializer.Serialize(analysis, JsonOptions);

        saved.Add(NewRequirement(project.ProjectId, actorId, ++sort, "AI brief analysis", request.Brief.Trim(), RequirementCategory.General, PriorityLevel.Medium, analysisJson));
        foreach (var room in analysis.Rooms.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            saved.Add(NewRequirement(project.ProjectId, actorId, ++sort, room, null, RequirementCategory.Room, PriorityLevel.High, null));
        }

        if (!string.Equals(analysis.Style, "Unspecified", StringComparison.OrdinalIgnoreCase))
        {
            saved.Add(NewRequirement(project.ProjectId, actorId, ++sort, analysis.Style, null, RequirementCategory.Style, PriorityLevel.Medium, null));
        }

        foreach (var constraint in analysis.Constraints)
        {
            saved.Add(NewRequirement(project.ProjectId, actorId, ++sort, constraint, null, RequirementCategory.Constraint, PriorityLevel.Medium, null));
        }

        foreach (var requirement in saved)
        {
            await _projects.AddRequirementAsync(requirement, cancellationToken);
        }

        await LogAsync(actorId, project.ProjectId, request.Brief, null, "AnalyzeBrief", analysisJson, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new AnalyzeBriefResponse(analysis, true, saved.Select(ProjectMapper.ToRequirement).ToList());
    }

    public async Task<PlanProjectResponse> PlanAsync(
        Guid projectId,
        PlanProjectRequest request,
        Guid actorId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken)
    {
        var project = await _projects.GetDetailAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");
        ProjectService.EnsureCanRead(project, actorId, roles);
        if (await _projects.HasPendingTaskPlanAsync(projectId, cancellationToken))
        {
            throw new ConflictException("A proposed AI task plan is already waiting for manager approval.");
        }

        var suggestions = await _ai.PlanTasksAsync(project.Name, cancellationToken);
        var tasks = suggestions.Select(suggestion => new ProjectTask
        {
            TaskId = Guid.NewGuid(),
            ProjectId = projectId,
            Title = suggestion.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Brief) ? "AI task plan. Not active until a manager approves it." : request.Brief.Trim(),
            Status = WorkTaskStatus.Proposed,
            Priority = EnumParser.ParsePriority(suggestion.Priority),
            Source = TaskSource.AiProposed,
            Labels = suggestion.Label,
            CreatedBy = actorId
        }).ToList();

        await _projects.AddTasksAsync(tasks, cancellationToken);
        var approval = new Approval
        {
            ApprovalId = Guid.NewGuid(),
            ProjectId = projectId,
            EntityType = ApprovalEntityType.TaskPlan,
            EntityId = projectId,
            Status = ApprovalStatus.Pending,
            RequestedById = actorId,
            Comments = "AI task plan is waiting for a project manager.",
            RequestedAt = DateTime.UtcNow,
            CreatedBy = actorId
        };
        await _projects.AddApprovalAsync(approval, cancellationToken);

        var tracked = await _projects.GetTrackedAsync(projectId, cancellationToken);
        if (tracked?.ProjectManagerId is Guid managerId)
        {
            await _notifications.AddAsync(new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = managerId,
                ProjectId = projectId,
                Title = "AI task plan needs approval",
                Message = $"{tasks.Count} proposed tasks are waiting. They are not active work yet.",
                Type = NotificationType.ApprovalRequested,
                RelatedEntityType = "Approval",
                RelatedEntityId = approval.ApprovalId,
                CreatedBy = actorId
            }, cancellationToken);
        }

        await LogAsync(actorId, projectId, request.Brief ?? project.Name, null, "PlanProject", JsonSerializer.Serialize(suggestions, JsonOptions), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new PlanProjectResponse(projectId, ApprovalStatus.Pending.ToString(), tasks.Select(ProjectMapper.ToTask).ToList());
    }

    public async Task<CommandPreviewDto> PreviewCommandAsync(PreviewCommandRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        var local = RevitCommandMapper.Match(request.Prompt);
        string? classified = local?.Command;
        if (local is null)
        {
            classified = await _ai.ClassifyCommandAsync(request.Prompt, cancellationToken);
        }

        var match = local ?? (RevitCommands.IsAllowed(classified)
            ? new CommandMatch(classified!, RevitCommands.CheckCodeFor(classified!)!)
            : null);

        var allowed = match is not null && RevitCommands.IsAllowed(match.Command);
        var log = new AiCommandLog
        {
            AiCommandLogId = Guid.NewGuid(),
            UserId = actorId,
            ProjectId = request.ProjectId,
            Prompt = request.Prompt.Trim(),
            DetectedCommand = match?.Command,
            ParametersJson = JsonSerializer.Serialize(new { request.ProjectId, checkCode = match?.CheckCode }, JsonOptions),
            PreviewJson = JsonSerializer.Serialize(new
            {
                command = match?.Command,
                impact = "Read-only search. No model elements will be changed. Results come only from stored validation data.",
                requiresConfirmation = true
            }, JsonOptions),
            Status = allowed ? AiCommandStatus.Previewed : AiCommandStatus.Failed,
            Provider = _ai.ProviderName,
            ResponseJson = allowed ? null : "Command is not on the whitelist.",
            CreatedBy = actorId
        };

        await _commands.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!allowed)
        {
            throw new AppValidationException("That request does not match an allowed Revit command. Allowed searches: missing fire ratings, room names, unplaced rooms, and sheet parameters.");
        }

        return new CommandPreviewDto(
            log.AiCommandLogId,
            log.DetectedCommand,
            log.Provider,
            true,
            "Read-only search. No model elements will be changed.",
            true,
            log.ParametersJson);
    }

    public async Task<ExecuteCommandResponse> ExecuteCommandAsync(ExecuteCommandRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        if (!request.Confirm)
        {
            throw new AppValidationException("Confirm must be true before a command can run.");
        }

        var log = await _commands.GetTrackedAsync(request.PreviewId, cancellationToken)
            ?? throw new NotFoundException("Command preview was not found.");
        if (log.UserId != actorId)
        {
            throw new ForbiddenException("Only the person who previewed this command can confirm it.");
        }

        if (log.Status != AiCommandStatus.Previewed || !RevitCommands.IsAllowed(log.DetectedCommand))
        {
            throw new ConflictException("This preview cannot be executed. Create a new preview for an allowed command.");
        }

        var checkCode = RevitCommands.CheckCodeFor(log.DetectedCommand!)!;
        var stored = await _revit.GetResultsForCheckAsync(log.ProjectId, checkCode, cancellationToken);
        var message = stored.Count == 0
            ? "No stored validation results match this check. The model has not been inspected for it, so no defects are reported."
            : $"{stored.Count} stored validation result(s) match {log.DetectedCommand}.";

        log.Status = AiCommandStatus.Executed;
        log.ResponseJson = JsonSerializer.Serialize(stored.Select(ProjectMapper.ToStoredCheck), JsonOptions);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ExecuteCommandResponse(
            log.AiCommandLogId,
            log.DetectedCommand!,
            log.Status.ToString(),
            message,
            stored.Select(ProjectMapper.ToStoredCheck).ToList());
    }

    public async Task<AskAiResponse> AskAsync(AskAiRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        var snapshot = await _business.GetSnapshotAsync(cancellationToken);
        var answer = BusinessAnswerBuilder.Answer(request.Question, snapshot);
        await LogAsync(actorId, null, request.Question, null, "BusinessQuestion", answer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new AskAiResponse(answer, _ai.ProviderName, "internal-services");
    }

    private async Task LogAsync(Guid userId, Guid? projectId, string prompt, string? command, string action, string response, CancellationToken cancellationToken)
    {
        await _commands.AddAsync(new AiCommandLog
        {
            AiCommandLogId = Guid.NewGuid(),
            UserId = userId,
            ProjectId = projectId,
            Prompt = prompt,
            DetectedCommand = command ?? action,
            Status = AiCommandStatus.Executed,
            Provider = _ai.ProviderName,
            ResponseJson = response,
            CreatedBy = userId
        }, cancellationToken);
    }

    private static ProjectRequirement NewRequirement(
        Guid projectId,
        Guid actorId,
        int sortOrder,
        string title,
        string? description,
        RequirementCategory category,
        PriorityLevel priority,
        string? analysisJson) => new()
    {
        RequirementId = Guid.NewGuid(),
        ProjectId = projectId,
        Title = title,
        Description = description,
        Category = category,
        Priority = priority,
        Source = RequirementSource.AiProposed,
        Status = RequirementStatus.Proposed,
        AiAnalysisJson = analysisJson,
        SortOrder = sortOrder,
        CreatedBy = actorId
    };
}

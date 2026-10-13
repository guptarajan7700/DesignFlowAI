using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Common;
using DesignFlow.Application.Contracts;
using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;

namespace DesignFlow.Application.Services;

public sealed class AutomationService : IAutomationService
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.Ordinal)
    {
        "TranslateModel",
        "DesignAutomation",
        "SheetExport",
        "Validation"
    };

    private readonly IAutomationRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;

    public AutomationService(IAutomationRepository jobs, IUnitOfWork unitOfWork)
    {
        _jobs = jobs;
        _unitOfWork = unitOfWork;
    }

    public async Task<AutomationJobDto> EnqueueAsync(CreateAutomationJobRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        var jobType = request.JobType.Trim();
        if (!AllowedTypes.Contains(jobType))
        {
            throw new AppValidationException("JobType must be TranslateModel, DesignAutomation, SheetExport, or Validation.");
        }

        var job = new AutomationJob
        {
            AutomationJobId = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            RevitModelId = request.RevitModelId,
            JobType = jobType,
            Status = AutomationJobStatus.Queued,
            ProgressPercent = 0,
            RequestPayload = request.RequestPayload,
            CreatedBy = actorId
        };

        await _jobs.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ProjectMapper.ToJob(job);
    }

    public async Task<AutomationJobDto> GetAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var job = await _jobs.GetAsync(jobId, cancellationToken)
            ?? throw new NotFoundException("Automation job was not found.");
        return ProjectMapper.ToJob(job);
    }
}

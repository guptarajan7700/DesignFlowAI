using System.Text.Json;
using DesignFlow.Application.Abstractions;
using DesignFlow.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DesignFlow.Infrastructure.Jobs;

public sealed class AutomationJobProcessor : IAutomationJobProcessor
{
    private readonly IAutomationRepository _jobs;
    private readonly IModelDerivativeClient _models;
    private readonly IDesignAutomationClient _designAutomation;
    private readonly IUnitOfWork _unitOfWork;

    public AutomationJobProcessor(
        IAutomationRepository jobs,
        IModelDerivativeClient models,
        IDesignAutomationClient designAutomation,
        IUnitOfWork unitOfWork)
    {
        _jobs = jobs;
        _models = models;
        _designAutomation = designAutomation;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        var job = await _jobs.GetNextQueuedTrackedAsync(cancellationToken);
        if (job is null)
        {
            return false;
        }

        job.Status = AutomationJobStatus.Running;
        job.ProgressPercent = 25;
        job.StartedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            if (job.JobType == "TranslateModel")
            {
                var urn = job.RevitModelId is Guid modelId
                    ? (await _jobs.GetModelAsync(modelId, cancellationToken))?.AutodeskUrn
                    : null;
                if (string.IsNullOrWhiteSpace(urn))
                {
                    job.ResultPayload = "{\"note\":\"No Autodesk URN is stored. Translation was not sent.\"}";
                }
                else
                {
                    var translation = await _models.TranslateAsync(urn, cancellationToken);
                    job.ResultPayload = JsonSerializer.Serialize(translation);
                    job.ExternalWorkItemId = translation.Urn;
                }
            }
            else
            {
                var note = await _designAutomation.SubmitWorkItemAsync(job.JobType, job.RequestPayload, cancellationToken);
                job.ResultPayload = JsonSerializer.Serialize(new { note });
            }

            job.Status = AutomationJobStatus.Succeeded;
            job.ProgressPercent = 100;
            job.CompletedAt = DateTime.UtcNow;
            job.ErrorMessage = null;
        }
        catch (Exception exception)
        {
            job.Status = AutomationJobStatus.Failed;
            job.ErrorMessage = exception.Message;
            job.CompletedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed class AutomationJobWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<AutomationJobWorker> _logger;

    public AutomationJobWorker(IServiceScopeFactory scopes, ILogger<AutomationJobWorker> logger)
    {
        _scopes = scopes;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IAutomationJobProcessor>();
                await processor.ProcessNextAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Automation worker failed while processing the job queue.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}

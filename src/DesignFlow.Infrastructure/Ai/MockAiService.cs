using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Ai;
using DesignFlow.Application.Contracts;

namespace DesignFlow.Infrastructure.Ai;

public sealed class MockAiService : IAiService
{
    public string ProviderName => "Mock";

    public Task<RequirementAnalysisDto> AnalyzeBriefAsync(string brief, CancellationToken cancellationToken) =>
        Task.FromResult(BriefAnalyzer.Analyze(brief));

    public Task<IReadOnlyList<SuggestedTaskDto>> PlanTasksAsync(string projectName, CancellationToken cancellationToken) =>
        Task.FromResult(BriefAnalyzer.Plan(projectName));

    public Task<string?> ClassifyCommandAsync(string prompt, CancellationToken cancellationToken) =>
        Task.FromResult(RevitCommandMapper.Match(prompt)?.Command);
}

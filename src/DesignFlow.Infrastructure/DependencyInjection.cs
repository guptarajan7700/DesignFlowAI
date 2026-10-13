using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Contracts;
using DesignFlow.Infrastructure.Ai;
using DesignFlow.Infrastructure.Autodesk;
using DesignFlow.Infrastructure.Jobs;
using DesignFlow.Infrastructure.Persistence;
using DesignFlow.Infrastructure.Security;
using DesignFlow.Infrastructure.Workspace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DesignFlow.Infrastructure;

public sealed class IntegrationStatusService : IIntegrationStatusService
{
    private readonly AiOptions _ai;
    private readonly AutodeskOptions _autodesk;

    public IntegrationStatusService(Microsoft.Extensions.Options.IOptions<AiOptions> ai, Microsoft.Extensions.Options.IOptions<AutodeskOptions> autodesk)
    {
        _ai = ai.Value;
        _autodesk = autodesk.Value;
    }

    public IntegrationStatusDto GetStatus()
    {
        var autodesk = _autodesk.IsConfigured ? "APS" : "Mock";
        return new IntegrationStatusDto(
            _ai.UseRemote ? _ai.Provider : "Mock",
            _autodesk.IsConfigured,
            autodesk,
            autodesk,
            "Stub",
            "Stub");
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DesignFlow");
        services.AddDbContext<DesignFlowDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddMemoryCache();
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.Configure<AutodeskOptions>(configuration.GetSection(AutodeskOptions.SectionName));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IClientRepository, EfClientRepository>();
        services.AddScoped<IProjectRepository, EfProjectRepository>();
        services.AddScoped<IIssueRepository, EfIssueRepository>();
        services.AddScoped<IRevitRepository, EfRevitRepository>();
        services.AddScoped<IAutomationRepository, EfAutomationRepository>();
        services.AddScoped<IAiCommandRepository, EfAiCommandRepository>();
        services.AddScoped<INotificationRepository, EfNotificationRepository>();
        services.AddScoped<IAuditRepository, EfAuditRepository>();
        services.AddScoped<IBusinessReadStore, EfBusinessReadStore>();
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddScoped<IIntegrationStatusService, IntegrationStatusService>();
        services.AddScoped<IAutomationJobProcessor, AutomationJobProcessor>();
        services.AddScoped<IWorkspaceService, WorkspaceService>();

        var ai = configuration.GetSection(AiOptions.SectionName).Get<AiOptions>() ?? new AiOptions();
        if (ai.UseRemote)
        {
            services.AddHttpClient<IAiService, OpenAiCompatibleService>(client =>
            {
                client.BaseAddress = new Uri(ai.Endpoint.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
        }
        else
        {
            services.AddSingleton<IAiService, MockAiService>();
        }

        var autodesk = configuration.GetSection(AutodeskOptions.SectionName).Get<AutodeskOptions>() ?? new AutodeskOptions();
        if (autodesk.IsConfigured)
        {
            services.AddHttpClient<IApsTokenProvider, ApsTokenProvider>(client =>
            {
                client.BaseAddress = new Uri("https://developer.api.autodesk.com/");
            });
            services.AddHttpClient<IModelDerivativeClient, ModelDerivativeClient>(client =>
            {
                client.BaseAddress = new Uri("https://developer.api.autodesk.com/");
            });
            services.AddHttpClient<IApsViewerService, ApsViewerService>(client =>
            {
                client.BaseAddress = new Uri("https://developer.api.autodesk.com/");
                client.Timeout = TimeSpan.FromMinutes(3);
            });
        }
        else
        {
            services.AddSingleton<IApsTokenProvider, MockApsTokenProvider>();
            services.AddSingleton<IModelDerivativeClient, MockModelDerivativeClient>();
            services.AddSingleton<IApsViewerService, UnavailableApsViewerService>();
        }

        services.AddSingleton<IDataManagementClient, StubDataManagementClient>();
        services.AddSingleton<IDesignAutomationClient, StubDesignAutomationClient>();
        services.AddSingleton<IApsWebhookRegistry, StubApsWebhookRegistry>();
        services.AddHostedService<AutomationJobWorker>();
        return services;
    }
}

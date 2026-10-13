using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DesignFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IIssueService, IssueService>();
        services.AddScoped<IRevitService, RevitService>();
        services.AddScoped<IAutomationService, AutomationService>();
        services.AddScoped<IAssistantService, AssistantService>();
        services.AddScoped<IApsWebhookService, ApsWebhookService>();
        services.AddValidatorsFromAssemblyContaining<AuthService>();
        return services;
    }
}

using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Contracts;
using DesignFlow.Domain;
using DesignFlow.Infrastructure.Autodesk;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DesignFlow.Api.Controllers;

/// <summary>Requirement analysis, task planning, whitelisted Revit commands, and firm questions.</summary>
[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/ai")]
public sealed class AiController : ControllerBase
{
    private readonly IAssistantService _assistant;
    private readonly ICurrentUser _currentUser;

    public AiController(IAssistantService assistant, ICurrentUser currentUser)
    {
        _assistant = assistant;
        _currentUser = currentUser;
    }

    /// <summary>Turns a free-text brief into rooms, style, constraints, suggested tasks, and a review checklist.</summary>
    [HttpPost("requirements/analyze")]
    [ProducesResponseType(typeof(AnalyzeBriefResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AnalyzeBriefResponse>> Analyze(AnalyzeBriefRequest request, CancellationToken cancellationToken) =>
        Ok(await _assistant.AnalyzeAsync(request, _currentUser.UserId, cancellationToken));

    /// <summary>Previews a whitelisted Revit command. Nothing is executed.</summary>
    [HttpPost("commands/preview")]
    [ProducesResponseType(typeof(CommandPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommandPreviewDto>> Preview(PreviewCommandRequest request, CancellationToken cancellationToken) =>
        Ok(await _assistant.PreviewCommandAsync(request, _currentUser.UserId, cancellationToken));

    /// <summary>Runs a previously previewed command after Confirm is true. Results come from stored validation rows.</summary>
    [HttpPost("commands/execute")]
    [ProducesResponseType(typeof(ExecuteCommandResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ExecuteCommandResponse>> Execute(ExecuteCommandRequest request, CancellationToken cancellationToken) =>
        Ok(await _assistant.ExecuteCommandAsync(request, _currentUser.UserId, cancellationToken));

    /// <summary>Answers a firm question by reading projects, tasks, and issues. It does not guess.</summary>
    [HttpPost("ask")]
    [ProducesResponseType(typeof(AskAiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AskAiResponse>> Ask(AskAiRequest request, CancellationToken cancellationToken) =>
        Ok(await _assistant.AskAsync(request, _currentUser.UserId, cancellationToken));
}

/// <summary>Receives Autodesk Platform Services webhook callbacks.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/webhooks")]
public sealed class WebhooksController : ControllerBase
{
    private readonly IApsWebhookService _webhooks;
    private readonly AutodeskOptions _options;
    private readonly IHostEnvironment _environment;

    public WebhooksController(IApsWebhookService webhooks, IOptions<AutodeskOptions> options, IHostEnvironment environment)
    {
        _webhooks = webhooks;
        _options = options.Value;
        _environment = environment;
    }

    /// <summary>Accepts an APS event. When a URN finishes extraction, the matching model is marked Ready.</summary>
    [HttpPost("aps")]
    [ProducesResponseType(typeof(ApsWebhookAccepted), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApsWebhookAccepted>> Receive(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            if (!Request.Headers.TryGetValue("X-DesignFlow-Webhook-Secret", out var sent)
                || sent != _options.WebhookSecret)
            {
                return Unauthorized();
            }
        }
        else if (!_environment.IsDevelopment())
        {
            return Unauthorized();
        }

        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        return Ok(await _webhooks.ReceiveAsync(payload, cancellationToken));
    }
}

/// <summary>Shows whether AI and Autodesk calls are live or using local stand-ins.</summary>
[ApiController]
[Authorize(Roles = RoleNames.Managers)]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    private readonly IIntegrationStatusService _status;

    public SystemController(IIntegrationStatusService status) => _status = status;

    /// <summary>Returns provider mode. Secrets are never included.</summary>
    [HttpGet("integrations")]
    [ProducesResponseType(typeof(IntegrationStatusDto), StatusCodes.Status200OK)]
    public ActionResult<IntegrationStatusDto> Integrations() => Ok(_status.GetStatus());
}

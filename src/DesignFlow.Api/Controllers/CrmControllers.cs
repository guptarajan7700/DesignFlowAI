using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Contracts;
using DesignFlow.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DesignFlow.Api.Controllers;

/// <summary>Issues a JWT and registers Client or Designer accounts.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Authenticates an active user and returns a bearer token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken) =>
        Ok(await _auth.LoginAsync(request, cancellationToken));

    /// <summary>Creates a Client or Designer account. Admin and ProjectManager accounts are not self-serve.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserProfileDto>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = await _auth.RegisterAsync(request, cancellationToken);
        return Created($"/api/auth/users/{user.UserId}", user);
    }
}

/// <summary>Firm client accounts.</summary>
[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/clients")]
public sealed class ClientsController : ControllerBase
{
    private readonly IClientService _clients;
    private readonly ICurrentUser _currentUser;

    public ClientsController(IClientService clients, ICurrentUser currentUser)
    {
        _clients = clients;
        _currentUser = currentUser;
    }

    /// <summary>Lists clients ordered by company name.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClientDto>>> List(CancellationToken cancellationToken) =>
        Ok(await _clients.ListAsync(cancellationToken));

    /// <summary>Creates a client.</summary>
    [Authorize(Roles = RoleNames.Managers)]
    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ClientDto>> Create(CreateClientRequest request, CancellationToken cancellationToken)
    {
        var client = await _clients.CreateAsync(request, _currentUser.UserId, cancellationToken);
        return Created($"/api/clients/{client.ClientId}", client);
    }
}

/// <summary>Projects, requirements, task-plan approval, and reports.</summary>
[ApiController]
[Authorize]
[Route("api/projects")]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;
    private readonly IAssistantService _assistant;
    private readonly ICurrentUser _currentUser;

    public ProjectsController(IProjectService projects, IAssistantService assistant, ICurrentUser currentUser)
    {
        _projects = projects;
        _assistant = assistant;
        _currentUser = currentUser;
    }

    /// <summary>Lists projects. Client users only see projects linked to their portal login.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectListDto>>> List(CancellationToken cancellationToken) =>
        Ok(await _projects.ListAsync(_currentUser.UserId, _currentUser.Roles, cancellationToken));

    /// <summary>Returns one project with requirements, milestones, tasks, and models.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await _projects.GetAsync(id, _currentUser.UserId, _currentUser.Roles, cancellationToken));

    /// <summary>Creates a project for an existing client.</summary>
    [Authorize(Roles = RoleNames.Managers)]
    [HttpPost]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProjectDetailDto>> Create(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _projects.CreateAsync(request, _currentUser.UserId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = project.ProjectId }, project);
    }

    /// <summary>Adds a requirement that is already approved scope.</summary>
    [Authorize(Roles = RoleNames.Staff)]
    [HttpPost("{id:guid}/requirements")]
    [ProducesResponseType(typeof(RequirementDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<RequirementDto>> AddRequirement(Guid id, CreateRequirementRequest request, CancellationToken cancellationToken)
    {
        var requirement = await _projects.AddRequirementAsync(id, request, _currentUser.UserId, _currentUser.Roles, cancellationToken);
        return Created($"/api/projects/{id}/requirements/{requirement.RequirementId}", requirement);
    }

    /// <summary>Promotes proposed AI tasks to ToDo. Requires a manager.</summary>
    [Authorize(Roles = RoleNames.Managers)]
    [HttpPost("{id:guid}/task-plans/approve")]
    [ProducesResponseType(typeof(PlanProjectResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlanProjectResponse>> Approve(Guid id, ReviewTaskPlanRequest? request, CancellationToken cancellationToken) =>
        Ok(await _projects.ApproveTaskPlanAsync(id, _currentUser.UserId, request?.Comments, cancellationToken));

    /// <summary>Rejects proposed AI tasks so they never join the active board.</summary>
    [Authorize(Roles = RoleNames.Managers)]
    [HttpPost("{id:guid}/task-plans/reject")]
    [ProducesResponseType(typeof(PlanProjectResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlanProjectResponse>> Reject(Guid id, ReviewTaskPlanRequest? request, CancellationToken cancellationToken) =>
        Ok(await _projects.RejectTaskPlanAsync(id, _currentUser.UserId, request?.Comments, cancellationToken));

    /// <summary>Builds a project report. The narrative uses stored validation rows only.</summary>
    [HttpGet("{id:guid}/reports")]
    [ProducesResponseType(typeof(ProjectReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProjectReportDto>> Report(Guid id, CancellationToken cancellationToken) =>
        Ok(await _projects.GetReportAsync(id, _currentUser.UserId, _currentUser.Roles, cancellationToken));

    /// <summary>Saves an AI task plan as Proposed and waits for manager approval.</summary>
    [Authorize(Roles = RoleNames.Staff)]
    [HttpPost("{id:guid}/plan")]
    [ProducesResponseType(typeof(PlanProjectResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<PlanProjectResponse>> Plan(Guid id, PlanProjectRequest request, CancellationToken cancellationToken)
    {
        var plan = await _assistant.PlanAsync(id, request, _currentUser.UserId, _currentUser.Roles, cancellationToken);
        return Created($"/api/projects/{id}", plan);
    }
}

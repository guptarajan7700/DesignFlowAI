using DesignFlow.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DesignFlow.Api.Controllers;

/// <summary>Autodesk Viewer session for the model studio page.</summary>
[ApiController]
[Authorize]
[Route("api/revit/viewer")]
public sealed class ViewerController : ControllerBase
{
    private readonly IApsViewerService _viewer;

    public ViewerController(IApsViewerService viewer) => _viewer = viewer;

    /// <summary>Returns a view token and the translated model to display. The client secret stays on the server.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ViewerSessionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ViewerSessionDto>> Get([FromQuery] Guid? projectId, CancellationToken cancellationToken) =>
        Ok(await _viewer.GetSessionAsync(projectId, cancellationToken));
}

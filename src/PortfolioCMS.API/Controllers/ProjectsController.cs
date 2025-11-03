using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.DTOs.Common;
using PortfolioCMS.API.Services;
using System.Security.Claims;

namespace PortfolioCMS.API.Controllers;

/// <summary>
/// Project management endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ProjectsController : ControllerBase
{
    private readonly IContentService _contentService;
    private readonly IObservabilityService _observability;

    public ProjectsController(IContentService contentService, IObservabilityService observability)
    {
        _contentService = contentService;
        _observability = observability;
    }

    /// <summary>
    /// Get all active projects
    /// </summary>
    /// <returns>List of active projects</returns>
    /// <response code="200">Projects retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectDto>), 200)]
    public async Task<ActionResult<List<ProjectDto>>> GetProjects()
    {
        var projects = await _contentService.GetActiveProjectsAsync();
        return Ok(projects);
    }

    /// <summary>
    /// Get all projects including inactive ones (Admin only)
    /// </summary>
    /// <returns>List of all projects</returns>
    /// <response code="200">Projects retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpGet("all")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(List<ProjectDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<List<ProjectDto>>> GetAllProjects()
    {
        var projects = await _contentService.GetAllProjectsAsync();
        return Ok(projects);
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Project details</returns>
    /// <response code="200">Project retrieved successfully</response>
    /// <response code="404">Project not found</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectDto), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ProjectDto>> GetProject(int id)
    {
        var project = await _contentService.GetProjectByIdAsync(id);
        if (project == null)
        {
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = "Project not found"
            });
        }

        return Ok(project);
    }

    /// <summary>
    /// Create new project (Admin only)
    /// </summary>
    /// <param name="dto">Project creation data</param>
    /// <returns>Created project</returns>
    /// <response code="201">Project created successfully</response>
    /// <response code="400">Validation errors</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> CreateProject([FromBody] CreateProjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _contentService.CreateProjectAsync(dto, userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetProject), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update existing project (Admin only)
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="dto">Project update data</param>
    /// <returns>Updated project</returns>
    /// <response code="200">Project updated successfully</response>
    /// <response code="400">Validation errors</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Project not found</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _contentService.UpdateProjectAsync(id, dto, userId);
        
        if (!result.Success)
        {
            return result.Message == "Project not found" ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete project (Admin only)
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Project deleted successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="404">Project not found</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> DeleteProject(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _contentService.DeleteProjectAsync(id, userId);
        
        if (!result.Success)
        {
            return result.Message == "Project not found" ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }
}
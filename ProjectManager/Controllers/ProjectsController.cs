using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Project.Models;
using Services.Services;

namespace ProjectManager.Controllers;

[Route("[controller]")]
[ApiController]
public class ProjectsController : ControllerBase
{
  private readonly ProjectsService _service;

  public ProjectsController(ProjectsService service)
  {
    _service = service;
  }

  [HttpGet("GetAllProjects/{page:int}/{size:int}")]
  [Authorize]
  public async Task<ProjectListResponse> GetAllProjects(int page = 0, int size = 0) =>
    await _service.GetAllProjectsAsync(page, size);

  [HttpGet("GetProject")]
  [Authorize]
  public async Task<ProjectResponse> GetProjectAsync(int projectId) =>
  await _service.GetProjectAsync(projectId);

  [HttpPost("CreateProject")]
  [Authorize]
  public async Task<ProjectResponse> CreateProject(ProjectData projectData) =>
  await _service.CreateProjectAsync(projectData);

  [HttpPost("UpdateProject")]
  [Authorize]
  public async Task<ProjectResponse> UpdateProject(ProjectData projectData) =>
  await _service.UpdateProjectAsync(projectData);


  [HttpGet("DeleteProject")]
  [Authorize]
  public async Task<ProjectResponse> DeleteProject(int projectId) =>
  await _service.DeleteProjectAsync(projectId);

  [HttpPost("UpdateTaskProject")]
  [Authorize]
  public async Task<TaskResponse> UpdateTaskProject(ProjectTask projectTask) =>
  await _service.UpdateTaskProjectAsync(projectTask);

  /*  [HttpGet("GetAllProjectsByUserId")]
  //[Authorize(Roles = "Admin")]
  public async Task<ProjectListResponse> GetAllProjectsByUserId(int userId) =>
    await _service.GetAllProjectsByUserIdAsync(userId);*/
}

using System.Data;
using DB.Models;
using DB.Models.Project.Model;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Models;
using Models.Project.Models;
using Services.Interfaces;
namespace Services.Services;

public class ProjectsService : IProjectsService
{
  private ProjectContext dbContext;
  private readonly ILogger<ProjectsService> _logger;

  public ProjectsService(ProjectContext dbContext, ILogger<ProjectsService> logger)
  {
    this.dbContext = dbContext;
    _logger = logger;
  }

  public async Task<ProjectListResponse> GetAllProjectsAsync(int page = 0, int size = 0) //TODO: Add UserAuthorized
  {
    try
    {
      var projectDetails = await dbContext.TbProjects.AsNoTracking().Where(p => !p.Deleted).ToListAsync();
      if (projectDetails != null)
      {
        var projects = Mapper.Map<List<TbProject>, List<ProjectData>>(projectDetails);
        _logger.LogInformation("Total projects fetched from DB: {Count}", projectDetails.Count);

        _logger.LogInformation("Fetching projects: page {Page}, pageSize {PageSize}", page, size);

        projects = projects
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        return new ProjectListResponse() { Response = ResponseEnum.Success, Data = projects };
      }
      return new ProjectListResponse() { Response = ResponseEnum.NoDataFound };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to fetch projects: {Message}", ex.Message);
      return new ProjectListResponse() { Response = ResponseEnum.BadRequest, Error = ex.Message };
    }
  }

  public Task<ProjectResponse> CreateProjectAsync(ProjectData projectData)
  {
    try
    {
      using var transaction = dbContext.Database.BeginTransaction();
      if (projectData != null)
      {
        var dbProject = Mapper.Map<ProjectData, TbProject>(projectData);
        dbContext.TbProjects.Add(dbProject);
        dbContext.SaveChanges();

        transaction.Commit();
        _logger.LogInformation("Project created successfully with ID: {ProjectId}", dbProject.ProjectId);

        return Task.FromResult(new ProjectResponse() { Response = ResponseEnum.Success, Data = projectData });
      }
      _logger.LogWarning("CreateProjectAsync called with null projectData.");
      return Task.FromResult(new ProjectResponse() { Response = ResponseEnum.BadRequest });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to create project: {Message}", ex.Message);
      return Task.FromResult(new ProjectResponse() { Response = ResponseEnum.ServerError, Error = ex.Message });
    }
  }

  public async Task<ProjectResponse> GetProjectAsync(int projectId) //TODO: Add UserAuthorized
  {
    try
    {
      var projectDetails = await dbContext.TbProjects
     .AsNoTracking()
     .Include(p => p.TbTasks)
     .Where(p => p.ProjectId == projectId)
     .FirstOrDefaultAsync();

      if (projectDetails != null)
      {
        var projects = Mapper.Map<TbProject, ProjectData>(projectDetails);
        return new ProjectResponse() { Response = ResponseEnum.Success, Data = projects };
      }
      return new ProjectResponse() { Response = ResponseEnum.NoDataFound };
    }
    catch (Exception ex)
    {
      return new ProjectResponse() { Response = ResponseEnum.BadRequest, Error = ex.Message };
    }
  }

  public async Task<ProjectResponse> DeleteProjectAsync(int projectId)
  {
    try
    {
      //await using var transaction = await dbContext.Database.BeginTransactionAsync(); // no need for one tb on db
      _logger.LogInformation("Attempting to soft-delete project with ID: {ProjectId}", projectId);

      var projectDetails = await dbContext.TbProjects
        .Where(p => p.ProjectId == projectId)
        .FirstOrDefaultAsync();

      if (projectDetails != null)
      {
        projectDetails.Deleted = true; // Enable historical tracking of deleted projects
        await dbContext.SaveChangesAsync();
        //transaction.Commit();

        _logger.LogInformation("Project with ID {ProjectId} marked as deleted successfully.", projectId);
        return new ProjectResponse() { Response = ResponseEnum.Success };
      }
      _logger.LogWarning("Project with ID {ProjectId} not found. Delete aborted.", projectId);
      return new ProjectResponse() { Response = ResponseEnum.NoDataFound };
    }
    catch (Exception ex)
    {
      return new ProjectResponse() { Response = ResponseEnum.ServerError, Error = ex.Message };
    }
  }

  public async Task<ProjectResponse> UpdateProjectAsync(ProjectData projectData)
  {
    try
    {
      //using var transaction = dbContext.Database.BeginTransaction();
      IDbContextTransaction? transaction = null;

      if (dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
      {
        transaction = dbContext.Database.BeginTransaction();
      }
      var projectDetails = await dbContext.TbProjects
          .Include(p => p.TbTasks)
          .Where(p => p.ProjectId == projectData.ProjectId)
          .FirstOrDefaultAsync();

      if (projectDetails != null)
      {
        //var comper = EqualityComparer.ReferenceEquals(taskIdDetails, projectTask);

        projectDetails.ProjectName = projectData.ProjectName;
        projectDetails.Description = projectData.Description;

        dbContext.Entry(projectDetails);
        dbContext.SaveChanges();
        if (projectData.Tasks != null && projectData.Tasks.Count > 0)
        {
          foreach (var task in projectData.Tasks)
          {
            var dbTask = await dbContext.TbTasks.Where(t => t.ProjectId == projectData.ProjectId).ToListAsync();

            //Create new Tasks
            var taskCreate = projectData.Tasks.Where(task => dbTask.All(c => c.TaskItemId != task.TaskItemId)).ToList();
            foreach (var newTask in taskCreate)
            {
              CreateTaskProgect(newTask);
            }
            //Delete Tasks
            var taskToDelete = dbTask.Where(s => !projectData.Tasks.Any(d => d.TaskItemId == s.TaskItemId)).ToList();
            foreach (var taskD in taskToDelete)
            {
              dbContext.TbTasks.Remove(taskD); // If we want we can update: Deleted=True; 
            }

            //Update Tasks
            var taskToUpdate = dbTask
                .Where(s => projectData.Tasks
                    .Any(d => d.TaskItemId == s.TaskItemId && !Equals(s, d))).ToList();
            foreach (var oldTask in taskToUpdate)
            {
              var newTask = projectData.Tasks.First(pt => pt.TaskItemId == oldTask.TaskItemId);
              await UpdateTaskProjectAsync(newTask);
            }
          }
        }
        await dbContext.SaveChangesAsync();
        if (transaction != null)
          await transaction.CommitAsync();
        return new ProjectResponse() { Response = ResponseEnum.Success, Data = projectData };
      }
      _logger.LogWarning("No projects found in database.");
      return new ProjectResponse() { Response = ResponseEnum.NoDataFound };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "UpdateProjectAsync failed");
      return new ProjectResponse() { Response = ResponseEnum.ServerError, Error = ex.Message };
    }
  }

  public async Task<TaskResponse> UpdateTaskProjectAsync(ProjectTask projectTask) // TODO: Need to add comper
  {
    try
    {
      var taskIdDetails = await dbContext.TbTasks.Where(t => t.TaskItemId == projectTask.TaskItemId
      && t.ProjectId == projectTask.ProjectId)
        .FirstOrDefaultAsync();

      if (taskIdDetails != null)
      {
        //var comper = EqualityComparer.ReferenceEquals(taskIdDetails, projectTask);
        taskIdDetails.Title = projectTask.Title;
        taskIdDetails.Description = projectTask.Description;
        taskIdDetails.Status = (int)projectTask.StatusId;

        //Mapper.Map(projectTask, taskIdDetails);
        //dbContext.TbTasks.Update(taskIdDetails);
        await dbContext.SaveChangesAsync();

        return new TaskResponse() { Response = ResponseEnum.Success, Data = projectTask };
      }
      _logger.LogWarning("No projects found in database.");
      return new TaskResponse() { Response = ResponseEnum.NoDataFound };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, $"TaskProject {projectTask.TaskItemId} failed");
      return new TaskResponse() { Response = ResponseEnum.ServerError, Error = ex.Message };
    }
  }

  public TaskResponse CreateTaskProgect(ProjectTask projectTask)
  {
    try
    {
      if (projectTask != null)
      {
        var TaskDetails = new TbTask
        {
          ProjectId = projectTask.ProjectId,
          TaskItemId = projectTask.TaskItemId,
          Description = projectTask.Description,
          Status = (int)projectTask.StatusId,
          Title = projectTask.Title
        };
        dbContext.Add(TaskDetails);
        dbContext.SaveChanges();

        return new TaskResponse() { Response = ResponseEnum.Success, Data = projectTask };
      }
      _logger.LogWarning("No projects found in database.");
      return new TaskResponse() { Response = ResponseEnum.NoDataFound };
    }
    catch (Exception ex)
    {
      return new TaskResponse() { Response = ResponseEnum.ServerError, Error = ex.Message };
    }
  }

  /*  public async Task<ProjectListResponse> GetAllProjectsByUserIdAsync(int userId) //TODO: Add UserAuthorized
  {
    try
    {
      var projectDetails = await dbContext.TbProjects.AsNoTracking().Where(p => p.UserId == userId).ToListAsync();
      if (projectDetails != null)
      {
        var projects = Mapper.Map<List<TbProject>, List<ProjectData>>(projectDetails);
        return new ProjectListResponse() { Response = ResponseEnum.Success, Data = projects };
      }
      return new ProjectListResponse() { Response = ResponseEnum.NoDataFound };
    }
    catch (Exception ex)
    {
      return new ProjectListResponse() { Response = ResponseEnum.BadRequest, Error = ex.Message };
    }
  }*/
}

using DB.Models;
using DB.Models.Project.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using Models.Project.Models;
using Moq;
using Services.Services;

namespace UnitTest;

public class ProjectServiceTests
{
  private readonly ProjectsService _service;
  private readonly ProjectContext _context;

  public ProjectServiceTests()
  {
    var options = new DbContextOptionsBuilder<ProjectContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new ProjectContext(options);
    var loggerMock = new Mock<ILogger<ProjectsService>>();

    _service = new ProjectsService(_context, loggerMock.Object);
  }

  [Fact]
  public async Task GetProjectById_Result()
  {
    var projects = new List<TbProject>
    {
        new TbProject { ProjectId = 1, ProjectName = "Alpha", Description = "First", Deleted = false },
        new TbProject { ProjectId = 2, ProjectName = "Beta", Description = "Second", Deleted = false },
        new TbProject { ProjectId = 3, ProjectName = "Gamma", Description = "Third", Deleted = true },
        new TbProject { ProjectId = 4, ProjectName = "Delta", Description = "Fourth", Deleted = false }
    };

    _context.TbProjects.AddRange(projects);
    await _context.SaveChangesAsync();

    var result = await _service.GetProjectAsync(1);

    Assert.Equal(ResponseEnum.Success, result.Response);
  }


  [Fact]
  public async Task GetAllProjects_Result()
  {
    var projects = new List<TbProject>
    {
        new TbProject { ProjectId = 1, ProjectName = "Alpha", Description = "First", Deleted = false },
        new TbProject { ProjectId = 2, ProjectName = "Beta", Description = "Second", Deleted = false },
        new TbProject { ProjectId = 3, ProjectName = "Gamma", Description = "Third", Deleted = true },
        new TbProject { ProjectId = 4, ProjectName = "Delta", Description = "Fourth", Deleted = false }
    };

    _context.TbProjects.AddRange(projects);
    await _context.SaveChangesAsync();

    var result = await _service.GetAllProjectsAsync(1, 10);

    Assert.Equal(ResponseEnum.Success, result.Response);
    Assert.Equal(3, result.Data.Count);
    Assert.DoesNotContain(result.Data, p => p.ProjectName == "Gamma");
  }

  [Fact]
  public async Task GetProjectAsync_ProjectExists()
  {
    var project = new TbProject
    {
      ProjectId = 1,
      ProjectName = "Test Project",
      Description = "Project with Task",
      Deleted = false,
      TbTasks = new List<TbTask>
        {
            new TbTask
            {
                TaskItemId = 1,
                Title = "Sample Task",
                Description = "Task Description",
                Status = 1,
                ProjectId = 1
            }
        }
    };
    _context.TbProjects.Add(project);
    _context.SaveChanges();

    var result = await _service.GetAllProjectsAsync(1, 10);

    Assert.Equal(ResponseEnum.Success, result.Response);
    Assert.Single(result.Data);
  }

  [Fact]
  public async Task DeleteProject_Result()
  {
    var projects = new List<TbProject>
    {
        new TbProject { ProjectId = 1, ProjectName = "Alpha", Description = "First", Deleted = false },
        new TbProject { ProjectId = 2, ProjectName = "Beta", Description = "Second", Deleted = false },
        new TbProject { ProjectId = 3, ProjectName = "Gamma", Description = "Third", Deleted = true },
        new TbProject { ProjectId = 4, ProjectName = "Delta", Description = "Fourth", Deleted = false }
    };

    _context.TbProjects.AddRange(projects);
    await _context.SaveChangesAsync();

    var result = await _service.DeleteProjectAsync(1);
    Assert.Equal(ResponseEnum.Success, result.Response);

    var deletedProject = await _context.TbProjects.FindAsync(1);
    Assert.NotNull(deletedProject);
    Assert.True(deletedProject.Deleted);
  }

  [Fact]
  public async Task UpdateProjectAsync_Tasks()
  {
    var originalProject = new TbProject
    {
      ProjectId = 1,
      ProjectName = "Old Project",
      Description = "Old Description",
      Deleted = false,
      TbTasks = new List<TbTask>
        {
            new TbTask
            {
                ProjectId = 1,
                TaskItemId = 1,
                Title = "Old Task",
                Description = "Task Desc",
                Status = 1
            }
        }
    };

    _context.TbProjects.Add(originalProject);
    await _context.SaveChangesAsync();

    var updatedProject = new ProjectData
    {
      ProjectId = 1,
      ProjectName = "Updated Project",
      Description = "Updated Desc",
      Tasks = new List<ProjectTask>
        {
            new ProjectTask
            {
                ProjectId = 1,
                TaskItemId = 1,
                Title = "Updated Task",
                Description = "Updated Task Desc",
                StatusId = StatusIdEnum.done
            },
            new ProjectTask
            {
                ProjectId = 1,
                TaskItemId = 2, 
                Title = "New Task",
                Description = "New Task Desc",
                StatusId = StatusIdEnum.inProgress
            }
        }
    };

    var result = await _service.UpdateProjectAsync(updatedProject);

    Assert.Equal(ResponseEnum.Success, result.Response);

    var updated = await _context.TbProjects.Include(p => p.TbTasks).FirstAsync(p => p.ProjectId == 1);
    Assert.Equal("Updated Project", updated.ProjectName);
    Assert.Equal("Updated Desc", updated.Description);

    var oldTask = updated.TbTasks.First(t => t.TaskItemId == 1);
    Assert.Equal("Updated Task", oldTask.Title);
    Assert.Equal("Updated Task Desc", oldTask.Description);
    Assert.Equal((int)StatusIdEnum.done, oldTask.Status);

    var newTask = updated.TbTasks.FirstOrDefault(t => t.TaskItemId == 2);
    Assert.NotNull(newTask);
    Assert.Equal("New Task", newTask.Title);

    Assert.Equal(2, updated.TbTasks.Count);
  }

}

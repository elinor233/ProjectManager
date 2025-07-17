namespace Models.Project.Models;

public class ProjectData
{
  //public int UserId { get; set; }
  public int ProjectId { get; set; }
  public string ProjectName { get; set; } = null!;
  public string Description { get; set; } = null!;
  public bool Deleted { get; set; }
  public List<ProjectTask> Tasks { get; set; } = [];
}
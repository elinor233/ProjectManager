namespace Models.Project.Models;

public class ProjectTask
{
  public int ProjectId { get; set; }
  public string Title { get; set; } = null!;
  public string Description { get; set; } = null!;
  public int TaskItemId { get; set; }
  public StatusIdEnum StatusId { get; set; }
}

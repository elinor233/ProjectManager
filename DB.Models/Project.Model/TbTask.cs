using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Models.Project.Model;

public class TbTask
{
  public int TaskItemId { get; set; }
  public int ProjectId { get; set; }
  public string Title { get; set; } = null!;
  public string Description { get; set; } = null!;
  public int Status { get; set; }
  public virtual TbProject? TbProject { get; set; }
}

public class TbTaskConfiguration : IEntityTypeConfiguration<TbTask>
{
  public void Configure(EntityTypeBuilder<TbTask> builder)
  {
    builder.ToTable("TbTask");

    builder.HasKey(e => new { e.ProjectId, e.TaskItemId });
    builder.HasOne(e => e.TbProject).WithMany(d => d.TbTasks).HasForeignKey(e => e.ProjectId);

    builder.Property(e => e.Title)
           .HasMaxLength(100);
    builder.Property(e => e.Description).HasMaxLength(100);

  }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Models.Project.Model;

public class TbProject
{
  //public int UserId { get; set; }
  public int ProjectId { get; set; }
  public string ProjectName { get; set; } = null!;
  public string Description { get; set; } = null!;
  public bool Deleted { get; set; }
  public virtual ICollection<TbTask> TbTasks { get; set; } = new List<TbTask>();
}

public class TbProjectConfiguration : IEntityTypeConfiguration<TbProject>
{
  public void Configure(EntityTypeBuilder<TbProject> builder)
  {
    builder.ToTable("TbProject");

    builder.HasKey(e => e.ProjectId);

    builder.Property(e => e.ProjectId)
       .ValueGeneratedOnAdd();

    builder.Property(e => e.ProjectName).HasMaxLength(20);
    builder.Property(e => e.Description).HasMaxLength(100);
  }
}

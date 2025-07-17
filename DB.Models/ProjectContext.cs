using DB.Models.Project.Model;
using DB.Models.Users.Model;
using Microsoft.EntityFrameworkCore;

namespace DB.Models
{
  public partial class ProjectContext : DbContext
  {
    public ProjectContext(DbContextOptions<ProjectContext> options) : base(options) { }

    public virtual DbSet<TbProject> TbProjects { get; set; } = null!;
    public virtual DbSet<TbTask> TbTasks { get; set; } = null!;
    public virtual DbSet<TbUser> TbUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      new TbProjectConfiguration().Configure(modelBuilder.Entity<TbProject>());
      new TbTaskConfiguration().Configure(modelBuilder.Entity<TbTask>());
      new TbUserConfiguration().Configure(modelBuilder.Entity<TbUser>());
    }
  }
}

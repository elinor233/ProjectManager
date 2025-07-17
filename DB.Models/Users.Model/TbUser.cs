using DB.Models.Project.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Models.Users.Model;

public class TbUser
{
  public int UserId { get; set; }
  public string Username { get; set; } = string.Empty;
  public  string Password { get; set; } = string.Empty;
  public int Role { get; set; } 
}
public class TbUserConfiguration : IEntityTypeConfiguration<TbUser>
{
  public void Configure(EntityTypeBuilder<TbUser> builder)
  {
    builder.ToTable("TbUser");

    builder.HasKey(e => new {e.UserId });
    builder.Property(e => e.Username).HasMaxLength(10);
    builder.Property(e => e.Password).HasMaxLength(10);
  }
}
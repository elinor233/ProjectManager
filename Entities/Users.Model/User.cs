namespace Models.Users.Model;

public class User
{
  public string Username { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public RoleEnum Role { get; set; }
}

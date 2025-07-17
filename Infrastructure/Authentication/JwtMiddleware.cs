using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

public class JwtMiddleware
{
  private readonly IConfiguration _config;
  public JwtMiddleware(IConfiguration config)
  {
    _config = config;
  }
  public string GenerateJwtToken(string userName, string[] roles)
  {
    var jwtSettings = _config.GetSection("Jwt");
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
      {
          new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
          new Claim(JwtRegisteredClaimNames.Email, userName),
          new Claim("username", userName)
      };

    foreach (var role in roles)
      claims.Add(new Claim("custom:role", role)); 

    var token = new JwtSecurityToken(
        issuer: jwtSettings["Issuer"],
        audience: jwtSettings["Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiresInMinutes"])),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

}

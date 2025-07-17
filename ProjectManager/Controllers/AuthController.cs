using Amazon.CognitoIdentityProvider.Model;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Services.Services;

namespace ProjectManager.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
  private readonly CognitoService _authService;
  public AuthController(CognitoService authService)
  {
    _authService = authService;
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginRequest request)
  {
    try
    {
      var token = await _authService.LoginAsync(request.Email, request.Password);
      return Ok(new { token });
    }
    catch (NotAuthorizedException)
    {
      return Unauthorized("Invalid credentials.");
    }
    catch (UserNotConfirmedException)
    {
      return BadRequest("User is not confirmed.");
    }
    catch (Exception ex)
    {
      return StatusCode(500, ex.Message);
    }
  }
}


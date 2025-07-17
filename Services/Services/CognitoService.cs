using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.Extensions.Configuration;
using Infrastructure.Authentication;

namespace Services.Services;

public class CognitoService
{
  private readonly AmazonCognitoIdentityProviderClient _provider;
  private readonly CognitoUserPool _userPool;
  private readonly IConfiguration _config;

  public CognitoService(IConfiguration config)
  {
    _config = config;
    var region = RegionEndpoint.GetBySystemName(_config["Cognito:Region"]);

    _provider = new AmazonCognitoIdentityProviderClient(region);
    _userPool = new CognitoUserPool(
        config["Cognito:UserPoolId"],
        config["Cognito:ClientId"],
        _provider
    );
  }
  public async Task<string?> LoginAWSAsync(string username, string password)
  {
    try
    {
      var user = new CognitoUser(username, _config["Cognito:ClientId"], _userPool, _provider);
      var authRequest = new InitiateSrpAuthRequest
      {
        Password = password
      };

      var authResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);
      return authResponse.AuthenticationResult.IdToken;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Login failed: {ex.Message}");
      return null;
    }
  }

  public async Task<string?> LoginAsync(string username, string password)
  {
    try
    {
      if (username != "testuser" || password != "123456")
        return null;
      var roles = new[] { "Admin", "User" };
      var jwt = new JwtMiddleware(_config);
      var token = jwt.GenerateJwtToken(username, roles);

      return await Task.FromResult(token);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Login failed: {ex.Message}");
      return null;
    }
  }
}

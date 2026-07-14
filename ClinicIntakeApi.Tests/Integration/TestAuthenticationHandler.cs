using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicIntakeApi.Tests.Integration;

/// <summary>
/// Provides predictable users during integration tests.
///
/// This handler only exists in the test project.
/// The real application continues to use JWT authentication.
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    // Give the test authentication scheme one reusable name.
    public const string SchemeName = "Test";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    )
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Read the test token from the Authorization header.
        string? authorizationHeader = Request.Headers.Authorization.FirstOrDefault();

        // No token means the test request is anonymous.
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        string userName;
        string userRole;

        // Create a predictable Admin for tests.
        if (authorizationHeader == "Bearer demo-token")
        {
            userName = "Test Admin";
            userRole = "Admin";
        }
        // Create a predictable ordinary User for tests.
        else if (authorizationHeader == "Bearer demo-user-token")
        {
            userName = "Test User";
            userRole = "User";
        }
        // Any other token is invalid.
        else
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid test authentication token."));
        }

        Claim[] claims =
        [
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, userRole),
            new Claim("ClinicId", "1"),
        ];

        var identity = new ClaimsIdentity(claims, SchemeName);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

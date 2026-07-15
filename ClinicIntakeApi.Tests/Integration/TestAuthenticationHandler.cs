using System.Security.Claims;
using System.Text.Encodings.Web;
using ClinicIntakeApi.Authorization;
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

    // Integration tests may use this header to choose
    // which clinic the fake authenticated user belongs to.
    public const string ClinicIdHeaderName = "X-Test-Clinic-Id";

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
        string? clinicId;

        // Create a predictable Admin for tests.
        if (authorizationHeader == "Bearer demo-token")
        {
            userName = "Test Admin";
            userRole = "Admin";
            clinicId = "1";
        }
        // Create a predictable ordinary User for tests.
        else if (authorizationHeader == "Bearer demo-user-token")
        {
            userName = "Test User";
            userRole = "User";
            clinicId = "1";
        }
        // Create an authenticated user with no ClinicId claim.
        else if (authorizationHeader == "Bearer demo-no-clinic-token")
        {
            userName = "Test User Without Clinic";
            userRole = "User";
            clinicId = null;
        }
        // Create an authenticated user with an invalid ClinicId.
        else if (authorizationHeader == "Bearer demo-invalid-clinic-token")
        {
            userName = "Test User With Invalid Clinic";
            userRole = "User";
            clinicId = "not-a-number";
        }
        // Any other token is invalid.
        else
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid test authentication token."));
        }
        // Most existing tests use the clinic assigned by their token.
        //
        // A clinic-isolation test may override that value so it can
        // create users for whichever clinic IDs exist in the test database.
        if (Request.Headers.TryGetValue(ClinicIdHeaderName, out var clinicIdHeader))
        {
            clinicId = clinicIdHeader.FirstOrDefault();
        }
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Role, userRole),
        };

        // Only add the claim when this test user has one.
        if (clinicId is not null)
        {
            claims.Add(new Claim(CustomClaimTypes.ClinicId, clinicId));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// Provides Claim, ClaimsIdentity, and ClaimsPrincipal.
// These classes describe who the user is.
using System.Security.Claims;
// Provides UrlEncoder.
// ASP.NET supplies this dependency to the authentication handler.
using System.Text.Encodings.Web;
// Provides ASP.NET authentication classes such as:
// AuthenticationHandler, AuthenticateResult, and AuthenticationTicket.
using Microsoft.AspNetCore.Authentication;
// Provides IOptionsMonitor.
// ASP.NET uses it to supply authentication configuration.
using Microsoft.Extensions.Options;

namespace ClinicIntakeApi.Authentication;

/// <summary>
/// Checks incoming requests for our temporary demo token.
///
/// This is fake authentication for learning purposes.
/// A real application would normally validate a JWT or use
/// an external identity provider.
/// </summary>
public class DemoAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>
    /// ASP.NET creates this handler using dependency injection.
    ///
    /// We do not manually create the options, logger, or encoder.
    /// ASP.NET provides them when it creates the handler.
    /// </summary>
    public DemoAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    )
        // Send these dependencies to the parent AuthenticationHandler class.
        : base(options, logger, encoder) { }

    /// <summary>
    /// ASP.NET calls this method when it tries to authenticate a request.
    ///
    /// The method checks the Authorization header and returns one of:
    ///
    /// NoResult  = No authentication information was provided.
    /// Fail      = Authentication information was provided but was invalid.
    /// Success   = The token was valid and a user was created.
    /// </summary>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Read the Authorization header from the incoming HTTP request.
        //
        // Example header:
        // Authorization: Bearer demo-token
        //
        // FirstOrDefault() returns the first header value.
        // If the header does not exist, it returns null.
        string? authorizationHeader = Request.Headers.Authorization.FirstOrDefault();

        // If no Authorization header was sent, there is nothing to validate.
        //
        // NoResult() means:
        // "This request did not provide authentication information."
        //
        // If the endpoint has [Authorize], ASP.NET will reject the request
        // with 401 Unauthorized.
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // The request included a header, so now check its exact value.
        //
        // This is intentionally simple for learning.
        // Real authentication should never use a hard-coded shared token.
        if (authorizationHeader != "Bearer demo-token")
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid authentication token."));
        }

        // The token is valid, so create claims describing the user.
        //
        // A claim is one fact about a user:
        // - Their name
        // - Their role
        // - The clinic they belong to
        Claim[] claims =
        [
            new Claim(ClaimTypes.Name, "Demo User"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("ClinicId", "1"),
        ];

        // Create an identity from the claims.
        //
        // The identity answers:
        // "Who is this user, and how were they authenticated?"
        //
        // Scheme.Name will be "Demo" because that is the authentication
        // scheme name we registered in Program.cs.
        var identity = new ClaimsIdentity(claims, Scheme.Name);

        // Wrap the identity in a ClaimsPrincipal.
        //
        // The principal represents the complete authenticated user.
        // ASP.NET will eventually assign this object to:
        //
        // HttpContext.User
        var principal = new ClaimsPrincipal(identity);

        // Package the authenticated user into an authentication ticket.
        //
        // The ticket contains:
        // - The authenticated user
        // - The authentication scheme that authenticated them
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        // Tell ASP.NET authentication succeeded.
        //
        // The authenticated principal will become HttpContext.User,
        // allowing controllers and authorization rules to inspect it.
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

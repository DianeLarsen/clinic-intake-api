# Authentication in ASP.NET Core

## What Is Authentication?

Authentication answers one question:

> Who are you?

Authorization answers a different question:

> What are you allowed to do?

This project first uses a fake token so the authentication process is easy to see:

```http
Authorization: Bearer demo-token
```

This is only a learning tool. A real application should not use a hard-coded shared token.

## Real-World Analogy

Think of the API as a secured medical clinic. A visitor arrives at the front desk and shows a badge—the `Authorization` header. The security guard, `DemoAuthenticationHandler`, examines it. No badge means the guard cannot identify the visitor (`NoResult`). A fake badge means identification fails (`Fail`). A valid badge containing `demo-token` tells the guard to create a record describing the visitor: their name, role, and clinic (`claims`). Those facts are combined into an identity, attached to the visitor (`ClaimsPrincipal`), and packaged into an approved entry pass (`AuthenticationTicket`). ASP.NET then carries that approved visitor record as `HttpContext.User`. When the visitor reaches a protected room, `[Authorize]` checks that they have a valid pass before letting them enter.

## The Basic Flow

```text
Request arrives
    ↓
Read the Authorization header
    ↓
Check the token
    ↓
Create facts about the user
    ↓
Create the user
    ↓
Store the user in HttpContext.User
    ↓
[Authorize] decides whether the request may enter the controller
```

The handler has three possible answers:

```text
No token       → NoResult: "I do not know who you are."
Wrong token    → Fail:     "Your badge is invalid."
Correct token  → Success:  "I identified you."
```

## Demo Authentication Handler

Create:

```text
Authentication/DemoAuthenticationHandler.cs
```

```csharp
// Provides Claim, ClaimsIdentity, and ClaimsPrincipal.
// These classes describe who the user is.
using System.Security.Claims;

// Provides UrlEncoder, which the parent authentication class requires.
using System.Text.Encodings.Web;

// Provides ASP.NET authentication classes.
using Microsoft.AspNetCore.Authentication;

// Provides IOptionsMonitor for authentication settings.
using Microsoft.Extensions.Options;

namespace ClinicIntakeApi.Authentication;

/// <summary>
/// Checks incoming requests for the temporary demo token.
/// This is fake authentication used only for learning.
/// </summary>
public class DemoAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>
    /// ASP.NET creates this handler using dependency injection.
    /// It supplies the settings, logger, and encoder.
    /// </summary>
    public DemoAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    )
        // Give these tools to the parent AuthenticationHandler class.
        : base(options, logger, encoder)
    {
    }

    /// <summary>
    /// ASP.NET calls this method when it needs to identify a user.
    /// </summary>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Read the Authorization header from the incoming request.
        // Example: Authorization: Bearer demo-token
        // The value may be null if the header was not sent.
        string? authorizationHeader =
            Request.Headers.Authorization.FirstOrDefault();

        // No header means nobody tried to identify themselves.
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult()
            );
        }

        // A header was sent, but it does not contain the accepted token.
        if (authorizationHeader != "Bearer demo-token")
        {
            return Task.FromResult(
                AuthenticateResult.Fail(
                    "Invalid authentication token."
                )
            );
        }

        // The token is correct. Create facts about the user.
        Claim[] claims =
        [
            new Claim(ClaimTypes.Name, "Demo User"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("ClinicId", "1"),
        ];

        // Group the user's facts into one identity.
        // Scheme.Name is "Demo", the method that verified the identity.
        var identity = new ClaimsIdentity(
            claims,
            Scheme.Name
        );

        // Create the complete user object ASP.NET understands.
        var principal = new ClaimsPrincipal(identity);

        // Package the user and authentication method into an entry pass.
        var ticket = new AuthenticationTicket(
            principal,
            Scheme.Name
        );

        // Tell ASP.NET that authentication succeeded.
        return Task.FromResult(
            AuthenticateResult.Success(ticket)
        );
    }
}
```

## The Code in Plain English

The entire method means:

```text
Read the token.

If there is no token:
    Say nobody tried to log in.

If the token is wrong:
    Say authentication failed.

If the token is correct:
    Write down facts about the user.
    Create the user.
    Give the user an entry pass.
    Say authentication succeeded.
```

## The Confusing Authentication Objects

### Claim

A claim is one fact about a user:

```text
Name = Demo User
Role = Admin
ClinicId = 1
```

Claims are similar to information printed on an employee badge.

### ClaimsIdentity

An identity groups the claims together and records how the claims were verified:

```text
Identity
├── Verified by: Demo
├── Name: Demo User
├── Role: Admin
└── ClinicId: 1
```

### ClaimsPrincipal

The `ClaimsPrincipal` is the complete user object ASP.NET understands.

For now, mentally translate this:

```csharp
ClaimsPrincipal
```

into this:

```text
the user
```

After authentication succeeds, ASP.NET makes the user available as:

```csharp
HttpContext.User
```

Inside a controller, it is available as:

```csharp
User
```

### AuthenticationTicket

The ticket is the approved entry pass. It packages together:

- The user
- The authentication method that approved the user

The nesting looks like this:

```text
Claims
  ↓ grouped into
Identity
  ↓ placed inside
Principal (the user)
  ↓ packaged inside
AuthenticationTicket
  ↓ returned as
Success
```

## Why Does the Constructor Look Strange?

```csharp
public DemoAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
)
    : base(options, logger, encoder)
{
}
```

ASP.NET creates the handler through dependency injection and gives it three tools:

| Tool | Simple meaning |
| --- | --- |
| `options` | Authentication settings |
| `logger` | A tool for recording activity and errors |
| `encoder` | A tool for safely handling text |

This line:

```csharp
: base(options, logger, encoder)
```

means:

> Give those tools to the parent `AuthenticationHandler` class.

Our constructor is empty because our class has no additional setup to perform.

## Why Use `Task.FromResult()`?

ASP.NET requires the method to return:

```csharp
Task<AuthenticateResult>
```

Real authentication may need to wait for another server or identity provider. Our fake handler already knows the answer immediately.

```csharp
Task.FromResult(result)
```

means:

> I already have the answer, but wrap it in a completed `Task` because ASP.NET requires one.

## Register the Handler in `Program.cs`

Creating the handler is not enough. It must be registered with ASP.NET:

```csharp
using ClinicIntakeApi.Authentication;
using Microsoft.AspNetCore.Authentication;
```

```csharp
builder.Services
    .AddAuthentication("Demo")
    .AddScheme<
        Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
        DemoAuthenticationHandler
    >(
        "Demo",
        options => { }
    );

builder.Services.AddAuthorization();
```

`"Demo"` is the name of this authentication method.

## Add Authentication to the Request Pipeline

Add these before `app.MapControllers()`:

```csharp
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

Order matters:

```text
UseAuthentication
    ↓ identifies the user

UseAuthorization
    ↓ checks the identified user's permissions

MapControllers
    ↓ runs the controller endpoint
```

Authorization cannot check a user until authentication has identified one.

## Protect a Controller

Add the authorization namespace:

```csharp
using Microsoft.AspNetCore.Authorization;
```

Then place `[Authorize]` on the controller:

```csharp
[Authorize]
public class RequestsController : ControllerBase
{
}
```

`[Authorize]` means:

> Only allow this request to continue if authentication created a valid user.

## Expected Test Results

### No token

```http
GET /api/v1/requests
```

Expected:

```text
401 Unauthorized
```

### Wrong token

```http
Authorization: Bearer wrong-token
```

Expected:

```text
401 Unauthorized
```

### Correct token

```http
Authorization: Bearer demo-token
```

Expected:

```text
200 OK
```

## Key Point to Remember

```text
Authentication = Who are you?
Authorization  = What are you allowed to do?
```

`DemoAuthenticationHandler` identifies the user. `[Authorize]` checks whether an identified user is required before allowing the request into the controller.

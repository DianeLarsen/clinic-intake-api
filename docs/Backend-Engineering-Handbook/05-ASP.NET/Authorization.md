# Authorization in ASP.NET Core

## What Is Authorization?

Authorization answers this question:

> What are you allowed to do?

Authentication and authorization have different jobs:

```text
Authentication = Who are you?
Authorization  = What are you allowed to do?
```

Authentication must happen first. ASP.NET cannot check a user's permissions until it knows who the user is.

## Real-World Analogy

Think of the API as a secured medical clinic. Authentication is the security guard at the entrance checking each person's badge and identifying them. Authorization is the lock on each room after they enter. A clinic employee with a valid badge may enter the main office, but only someone whose badge says `Admin` may enter the records-destruction room. A person without a valid badge receives `401 Unauthorized`. A person with a valid badge but the wrong role receives `403 Forbidden`. The first person was never identified; the second person was identified but did not have permission.

## HTTP Status Codes

### 401 Unauthorized

Despite its name, `401 Unauthorized` means the request was not successfully authenticated.

Common reasons include:

- The token is missing
- The token is invalid
- The token is expired

Plain-English meaning:

> I do not know who you are, so you cannot enter.

### 403 Forbidden

`403 Forbidden` means authentication succeeded, but the user does not have the required permission.

Plain-English meaning:

> I know who you are, but you are not allowed to do this.

The difference is:

```text
401 = No valid identity
403 = Valid identity, insufficient permission
```

## The Authorization Flow

```text
Request arrives
    ↓
Authentication checks the token
    ↓
Authentication creates the user and claims
    ↓
Authorization reads the endpoint's rules
    ↓
Authorization checks the user's claims
    ↓
Allowed  → Run the controller action
Denied   → Return 401 or 403
```

## Register Authorization

Authorization is registered in `Program.cs`:

```csharp
// Register ASP.NET's authorization services.
// These services check rules such as [Authorize]
// and required user roles.
builder.Services.AddAuthorization();
```

Add authorization middleware after authentication middleware:

```csharp
// First, determine who the user is.
app.UseAuthentication();

// Next, determine what the user may access.
app.UseAuthorization();

// Finally, run the matching controller action.
app.MapControllers();
```

The order matters:

```text
UseAuthentication
    ↓ creates HttpContext.User

UseAuthorization
    ↓ checks HttpContext.User

MapControllers
    ↓ runs the endpoint
```

Authorization cannot check a user until authentication has created one.

## Require an Authenticated User

Import the authorization attributes:

```csharp
using Microsoft.AspNetCore.Authorization;
```

Add `[Authorize]` to a controller:

```csharp
// Requires the request to contain valid authentication.
// If authentication does not create a valid user,
// ASP.NET stops the request and returns 401 Unauthorized.
[Authorize]
public class RequestsController : ControllerBase
{
}
```

When `[Authorize]` is placed on a controller, every action in that controller requires an authenticated user.

The attribute does not check the token itself. The authentication handler checks the token. `[Authorize]` only requires authentication to have succeeded.

## Roles

A role describes a category of user:

```text
Admin
User
Manager
Nurse
```

In the demo authentication handler, the role is stored as a claim:

```csharp
new Claim(ClaimTypes.Role, userRole)
```

The demo tokens create these roles:

| Token | User | Role |
| --- | --- | --- |
| `demo-token` | Demo Admin | Admin |
| `demo-user-token` | Demo User | User |

The simplified handler logic is:

```csharp
string userName;
string userRole;

if (authorizationHeader == "Bearer demo-token")
{
    userName = "Demo Admin";
    userRole = "Admin";
}
else if (authorizationHeader == "Bearer demo-user-token")
{
    userName = "Demo User";
    userRole = "User";
}
else
{
    return Task.FromResult(
        AuthenticateResult.Fail(
            "Invalid authentication token."
        )
    );
}

Claim[] claims =
[
    new Claim(ClaimTypes.Name, userName),
    new Claim(ClaimTypes.Role, userRole),
    new Claim("ClinicId", "1"),
];
```

## Require a Specific Role

The entire `RequestsController` requires authentication:

```csharp
[Authorize]
public class RequestsController : ControllerBase
```

The delete action has a stricter rule:

```csharp
// Requires an authenticated user whose Role claim is "Admin".
// An authenticated non-admin user receives 403 Forbidden.
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    bool deleted =
        await _intakeService.DeleteRequestAsync(id);

    return deleted ? NoContent() : NotFound();
}
```

These attributes combine to mean:

```text
Controller rule: You must be authenticated.
Delete rule:     You must also have the Admin role.
```

An ordinary authenticated user may access endpoints such as:

```http
GET /api/v1/requests
```

But that user may not access:

```http
DELETE /api/v1/requests/27
```

## How ASP.NET Finds the Role

The authentication handler creates this claim:

```csharp
new Claim(ClaimTypes.Role, "Admin")
```

The endpoint requires:

```csharp
[Authorize(Roles = "Admin")]
```

ASP.NET compares the required role with the user's role claim:

```text
Required role: Admin
User's role:   Admin
Result:        Allowed
```

For an ordinary user:

```text
Required role: Admin
User's role:   User
Result:        403 Forbidden
```

Role names must match. `Admin` and `admin` should not be treated as interchangeable names.

## Allow Public Access

Sometimes an endpoint should be available without authentication. A basic health check is a common example because monitoring systems may need to confirm that the API is running.

The controller can require authentication by default:

```csharp
[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/system")]
[Authorize]
public class SystemController : ControllerBase
```

Then one action can override that rule with `[AllowAnonymous]`:

```csharp
// Overrides [Authorize] for this specific endpoint.
// Anyone can check whether the API is running,
// even without an authentication token.
[AllowAnonymous]
[HttpGet("health")]
public IActionResult GetHealth()
{
    return Ok(new
    {
        status = "Healthy",
    });
}
```

The public endpoint is:

```http
GET /api/v1/system/health
```

`[AllowAnonymous]` means:

> Do not require an authenticated user for this action.

It overrides the surrounding `[Authorize]` rule.

## Authorization Examples

| Request | Identity | Result |
| --- | --- | --- |
| GET requests, no token | Unknown | `401 Unauthorized` |
| GET requests, invalid token | Unknown | `401 Unauthorized` |
| GET requests, User token | User | `200 OK` |
| DELETE request, User token | User | `403 Forbidden` |
| DELETE request, Admin token | Admin | Controller action runs |
| GET health, no token | Anonymous | `200 OK` |

## Integration Tests

Authorization should be tested through the complete HTTP request pipeline. These tests prove that authentication, claims, authorization attributes, routing, and controllers work together.

### Ordinary Users Can Read Requests

```csharp
[Fact]
public async Task GetRequests_WhenUserIsAuthenticated_ReturnsOk()
{
    // Arrange
    using HttpClient client = _factory.CreateClient();

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
            "Bearer",
            "demo-user-token"
        );

    // Act
    HttpResponseMessage response =
        await client.GetAsync("/api/v1/requests");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode
    );
}
```

### Ordinary Users Cannot Delete Requests

```csharp
[Fact]
public async Task DeleteRequest_WhenUserIsNotAdmin_ReturnsForbidden()
{
    // Arrange
    using HttpClient client = _factory.CreateClient();

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
            "Bearer",
            "demo-user-token"
        );

    // Act
    HttpResponseMessage response =
        await client.DeleteAsync(
            "/api/v1/requests/999999"
        );

    // Assert
    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode
    );
}
```

### Admin Users Reach the Delete Action

```csharp
[Fact]
public async Task DeleteRequest_WhenUserIsAdmin_ReachesController()
{
    // Arrange
    using HttpClient client = _factory.CreateClient();

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
            "Bearer",
            "demo-token"
        );

    // Act
    HttpResponseMessage response =
        await client.DeleteAsync(
            "/api/v1/requests/999999"
        );

    // Assert
    //
    // NotFound proves authorization allowed the request
    // into the controller. Request 999999 simply does not exist.
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode
    );
}
```

The Admin test expects `404 Not Found`, not `200 OK`, because the fake request does not exist. The important fact is that it did not return `403 Forbidden`. The Admin passed authorization and reached the controller.

### Anonymous Users Can Reach the Health Endpoint

```csharp
[Fact]
public async Task GetHealth_WhenTokenIsMissing_ReturnsOk()
{
    // Arrange
    // Create a client without an Authorization header.
    using HttpClient client = _factory.CreateClient();

    // Act
    HttpResponseMessage response =
        await client.GetAsync(
            "/api/v1/system/health"
        );

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode
    );
}
```

## What Authorization Does Not Do

Authorization does not:

- Check whether a password is correct
- Validate a token
- Create the user
- Read or write database records

Those belong to authentication or application logic.

Authorization only decides whether the identified user satisfies an endpoint's access rules.

## Key Points to Remember

```text
[Authorize]
    → Requires any authenticated user

[Authorize(Roles = "Admin")]
    → Requires an authenticated Admin

[AllowAnonymous]
    → Does not require authentication

401 Unauthorized
    → The user was not successfully identified

403 Forbidden
    → The user was identified but lacks permission
```

Authentication creates `HttpContext.User`. Authorization examines that user and decides whether the request may reach the controller action.

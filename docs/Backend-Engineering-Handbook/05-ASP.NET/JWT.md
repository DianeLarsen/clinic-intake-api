# JWT Authentication in ASP.NET Core

## What Is a JWT?

JWT means:

```text
JSON Web Token
```

A JWT is a signed token that an application can use to identify a user or another application.

It is commonly sent to an API in this HTTP header:

```http
Authorization: Bearer <token>
```

The word `Bearer` means that whoever possesses the token can present it. A bearer token must therefore be protected while it remains valid.

## Real-World Analogy

Think of a JWT as a clinic identification badge with a tamper-proof seal. The badge openly displays facts such as the employee's name, role, and clinic. Those facts are the claims. The seal is the signature. Anyone holding the badge can read what it says, but if someone changes `Role: User` to `Role: Admin`, the seal no longer matches. The API detects the change and rejects the badge.

## A JWT Has Three Parts

A JWT usually looks like a very long string:

```text
eyJhbGciOi...eyJzdWIiOi...SflKxwRJ...
```

It has three sections separated by periods:

```text
Header.Payload.Signature
```

### Header

The header describes the token and the algorithm used to sign it:

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

### Payload

The payload contains claims about the user and token:

```json
{
  "name": "Demo User",
  "role": "User",
  "ClinicId": "1",
  "iss": "dotnet-user-jwts",
  "aud": "http://localhost:5090",
  "exp": 1784050000
}
```

Claims used in this project include:

| Claim | Meaning |
| --- | --- |
| `name` | The user's display name |
| `role` | The user's authorization role |
| `ClinicId` | The clinic associated with the user |
| `iss` | The system that issued the token |
| `aud` | The API for which the token was created |
| `exp` | The time when the token expires |

### Signature

The signature allows the API to confirm that:

- A trusted issuer created the token
- The header was not changed
- The payload was not changed

If any signed content changes, the signature validation fails.

## JWTs Are Usually Not Encrypted

The header and payload are encoded, not hidden. Someone holding a normal JWT can decode and read them.

Never place information such as this in a JWT payload:

- Passwords
- Private signing keys
- Medical records
- Social Security numbers
- Other confidential information

The signature protects the token from undetected changes. It does not make the claims secret.

## Authentication Flow

```text
Identity system creates a signed JWT
    ↓
Client stores the JWT temporarily
    ↓
Client sends Authorization: Bearer <token>
    ↓
ASP.NET's JWT handler validates the token
    ↓
JWT claims become HttpContext.User
    ↓
Authorization checks [Authorize] and role rules
    ↓
Controller action runs or access is denied
```

## What the API Validates

The API should validate:

### Signature

> Was the token created by a trusted issuer, and was it changed afterward?

### Issuer

The issuer claim is named:

```text
iss
```

It answers:

> Who created this token?

### Audience

The audience claim is named:

```text
aud
```

It answers:

> Which API is supposed to accept this token?

For local development, this project uses:

```text
http://localhost:5090
```

### Expiration

The expiration claim is named:

```text
exp
```

It answers:

> Is this token still valid, or has its time run out?

An invalid signature, issuer, audience, or expiration produces:

```text
401 Unauthorized
```

## Install JWT Bearer Authentication

The project targets .NET 8, so it uses the .NET 8 version of the package:

```bash
dotnet add package \
  Microsoft.AspNetCore.Authentication.JwtBearer \
  --version 8.0.28
```

Then verify that the application builds:

```bash
dotnet build
```

## Register JWT Authentication

Add this import to `Program.cs`:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
```

Register the JWT bearer handler:

```csharp
// Register JWT bearer authentication.
//
// ASP.NET looks for a JWT in this header:
// Authorization: Bearer <token>
//
// The JWT handler validates the token and creates
// HttpContext.User from the token's claims.
builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme
    )
    .AddJwtBearer();

builder.Services.AddAuthorization();
```

`JwtBearerDefaults.AuthenticationScheme` is the built-in scheme name:

```text
Bearer
```

Keep the middleware in this order:

```csharp
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

Authentication must create `HttpContext.User` before authorization can inspect it.

## Create Local Development Tokens

The .NET SDK provides the `dotnet user-jwts` tool for local development tokens.

The tool stores its signing key outside the repository using .NET user secrets. It may add non-secret issuer and audience settings to `appsettings.Development.json`.

These local tokens are development tools. They are not a production identity system.

### Create an Admin token

```bash
ADMIN_TOKEN=$(dotnet user-jwts create \
  --name "Demo Admin" \
  --role Admin \
  --claim ClinicId=1 \
  --audience http://localhost:5090 \
  --valid-for 30d \
  --output token)
```

### Create an ordinary User token

```bash
USER_TOKEN=$(dotnet user-jwts create \
  --name "Demo User" \
  --role User \
  --claim ClinicId=1 \
  --audience http://localhost:5090 \
  --valid-for 30d \
  --output token)
```

Shell variables such as `ADMIN_TOKEN` and `USER_TOKEN` exist only in the terminal session where they were created.

## Check a Token Without Displaying It

A complete JWT has three sections:

```bash
awk -F. '{print "JWT sections:", NF}' \
  <<< "$ADMIN_TOKEN"
```

Expected:

```text
JWT sections: 3
```

A value such as this is not a complete JWT:

```text
eyJhbGciOi
```

It is only the beginning of the header.

## Test Authentication

### Valid Admin token

```bash
curl -i \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  http://localhost:5090/api/v1/requests
```

Expected:

```text
200 OK
```

### Invalid old demo token

```bash
curl -i \
  -H "Authorization: Bearer demo-token" \
  http://localhost:5090/api/v1/requests
```

Expected:

```text
401 Unauthorized
```

The old hard-coded token is no longer accepted by the real application.

## Test JWT Roles

### Ordinary User can read

```bash
curl -i \
  -H "Authorization: Bearer $USER_TOKEN" \
  http://localhost:5090/api/v1/requests
```

Expected:

```text
200 OK
```

### Ordinary User cannot delete

```bash
curl -i \
  -X DELETE \
  -H "Authorization: Bearer $USER_TOKEN" \
  http://localhost:5090/api/v1/requests/999999
```

Expected:

```text
403 Forbidden
```

The JWT is valid, but its role claim is `User` rather than `Admin`.

### Admin passes the delete authorization rule

```bash
curl -i \
  -X DELETE \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  http://localhost:5090/api/v1/requests/999999
```

Expected:

```text
404 Not Found
```

The `404` proves that the Admin passed authorization and reached the controller. Request `999999` simply does not exist.

## Configure Swagger for JWTs

Swagger needs to know:

- That the API uses Bearer authentication
- Which endpoints require authentication

Register a Bearer security definition in `Program.cs`:

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Paste the JWT only. Swagger adds the 'Bearer' prefix.",
        }
    );

    options.OperationFilter<
        AuthorizeCheckOperationFilter
    >();
});
```

The `AuthorizeCheckOperationFilter` inspects `[Authorize]` and `[AllowAnonymous]` on controller actions and marks protected controller endpoints with Swagger lock icons. Infrastructure health checks are mapped separately at `/health/live` and `/health/ready`, so they are not part of these controller operations.

## Enter a JWT in Swagger

Open:

```text
http://localhost:5090/swagger
```

Then:

1. Select **Authorize**.
2. Paste the complete JWT.
3. Do not include the word `Bearer`.
4. Select **Authorize** again.
5. Close the authorization dialog.

Swagger masks the saved value as:

```text
******
```

Swagger displaying `Authorized` only means it saved the text. Swagger does not validate the JWT in the dialog. The API validates the token when a request is sent.

The final request header should look like:

```http
Authorization: Bearer header.payload.signature
```

## Production and Test Authentication

The normal application uses:

```text
JwtBearerHandler
```

Integration tests use:

```text
TestAuthenticationHandler
```

The test handler exists only inside the test project. `CustomWebApplicationFactory` replaces the default authentication scheme when the environment is `Testing`.

```text
Normal application
    → Validates real signed JWTs

Integration tests
    → Creates predictable test users from test tokens
```

This lets controller tests reliably create Admin, User, invalid, and anonymous requests without placing a real signing key in the repository.

The test handler does not replace JWT authentication in the real application.

## What Not to Commit

Do not commit:

- JWT strings
- Signing keys
- Copied Authorization headers
- Secrets files
- Passwords used to obtain tokens

The following items are normally safe to commit:

- JWT bearer configuration code
- Issuer and audience names
- `UserSecretsId` in the project file
- Test-only fake authentication code in the test project

## Local Development Versus Production

`dotnet user-jwts` is appropriate for local development and testing. A production application should normally obtain access tokens from a standards-based identity system using OAuth 2.0 or OpenID Connect.

For an Azure-hosted API, a future production identity provider could be Microsoft Entra ID. The API would validate tokens issued by Entra instead of tokens issued by `dotnet user-jwts`.

The API should not invent its own production login and token-issuing system unless there is a strong, carefully reviewed reason to do so. Authentication code is a terrible place to express unearned creativity.

## Key Points to Remember

```text
JWT
    → A signed token containing claims

Header
    → Describes the token and signing algorithm

Payload
    → Contains readable claims

Signature
    → Detects tampering and proves who signed the token

Bearer
    → Whoever possesses the token can present it

401 Unauthorized
    → Token missing or invalid

403 Forbidden
    → Token valid, but permission is insufficient
```

JWT authentication creates the user from validated claims. Authorization then decides what that user is allowed to do.

## References

- [Configure JWT bearer authentication in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication)
- [Generate local tokens with dotnet user-jwts](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
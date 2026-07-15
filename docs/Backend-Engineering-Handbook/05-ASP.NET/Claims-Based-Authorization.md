# Claims-Based Authorization

## Problem

Authentication proves that a request contains a valid identity. It does not automatically prove that the identity may access a particular clinic or record.

The Clinic Intake API needs every request controller action to receive a valid clinic identity. That clinic identity is stored in the JWT as a custom claim:

```text
ClinicId = 1
```

## Claims

A claim is a name/value fact about an authenticated identity.

Examples:

```text
Name     = Diane
Role     = Admin
ClinicId = 1
```

Claims describe the identity. Authorization policies use those claims to decide whether the identity may enter an endpoint.

Claim names and values should use consistent casing. The API stores its custom claim name in one constant:

```csharp
public static class CustomClaimTypes
{
    public const string ClinicId = "ClinicId";
}
```

This avoids scattering the literal string `"ClinicId"` throughout the application.

## Authorization Policy

The API registers a `ClinicMember` policy:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.ClinicMember,
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireAssertion(context =>
                int.TryParse(
                    context.User
                        .FindFirst(CustomClaimTypes.ClinicId)
                        ?.Value,
                    out int clinicId
                )
                && clinicId > 0
            );
        }
    );
});
```

The policy requires:

1. A valid authenticated identity.
2. A `ClinicId` claim.
3. A claim value that can be converted to a positive integer.

Checking only that the claim exists is insufficient. A value such as `"not-a-number"` would pass a simple presence check and fail later when the application tried to use it.

## Applying the Policy

The policy is applied to the entire requests controller:

```csharp
[Authorize(Policy = AuthorizationPolicies.ClinicMember)]
public class RequestsController : ControllerBase
{
}
```

Every action in that controller requires the policy unless an action explicitly overrides the authorization behavior.

An action may add another requirement. The delete endpoint also requires the `Admin` role:

```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
}
```

Both rules must pass:

```text
Valid ClinicId + Admin role     -> Delete action may run
Valid ClinicId + ordinary User -> 403 Forbidden
Admin role + invalid ClinicId   -> 403 Forbidden
No valid authentication        -> 401 Unauthorized
```

## Reading the Clinic ID

JWT claim values are strings, but the database uses an integer `ClinicId`. A `ClaimsPrincipal` extension performs the conversion in one place:

```csharp
public static int GetRequiredClinicId(
    this ClaimsPrincipal user
)
{
    string? clinicIdValue =
        user.FindFirst(CustomClaimTypes.ClinicId)?.Value;

    if (
        !int.TryParse(clinicIdValue, out int clinicId)
        || clinicId <= 0
    )
    {
        throw new InvalidOperationException(
            "The authenticated user does not have a valid ClinicId claim."
        );
    }

    return clinicId;
}
```

Controllers can then use:

```csharp
int clinicId = User.GetRequiredClinicId();
```

The policy should reject a bad claim before the controller runs. The extension validates again so it fails closed if it is accidentally used without the policy.

## HTTP Status Codes

### 401 Unauthorized

The API could not establish a valid authenticated identity.

Examples:

- Missing bearer token
- Invalid token
- Expired token
- Invalid signature

Despite its name, `401 Unauthorized` normally means the request is not authenticated.

### 403 Forbidden

The API authenticated the identity, but the identity did not meet an authorization rule.

Examples:

- Missing `ClinicId` claim
- Invalid `ClinicId` claim
- Ordinary user attempting an Admin-only operation

### 404 Not Found

The authenticated identity passed the controller policy, but a requested record was not found inside its clinic boundary.

Returning `404` for a foreign-clinic record avoids revealing that the record exists.

## Local Development Tokens

The .NET user-JWT tool can create development tokens with custom claims:

```bash
dotnet user-jwts create \
  --name "Clinic 1 Admin" \
  --role Admin \
  --claim ClinicId=1
```

These tokens are for local development. A production system should receive access tokens from a trusted identity provider.

## Testing the Policy

The integration-test authentication handler creates predictable test identities:

```text
Valid ClinicId   -> Controller may run
Missing ClinicId -> 403 Forbidden
Invalid ClinicId -> 403 Forbidden
```

The fake handler exists only in the test project. Production continues to use JWT bearer authentication.

## Key Lesson

Authentication answers:

```text
Who is making this request?
```

Authorization answers:

```text
Does this identity satisfy the rules for this endpoint?
```

The `ClinicMember` policy ensures that the controller receives a usable clinic identity. The repository must still use that identity to isolate the data.

## References

- [Claim-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/claims)
- [Generate tokens with dotnet user-jwts](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)

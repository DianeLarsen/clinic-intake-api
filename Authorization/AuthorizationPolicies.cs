namespace ClinicIntakeApi.Authorization;

/// <summary>
/// Contains the names of the application's authorization policies.
/// </summary>
public static class AuthorizationPolicies
{
    // Requires an authenticated user with a valid ClinicId claim.
    public const string ClinicMember = "ClinicMember";
}

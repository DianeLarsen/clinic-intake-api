using System.Security.Claims;

namespace ClinicIntakeApi.Authorization;

/// <summary>
/// Provides reusable methods for reading application-specific
/// claims from an authenticated user.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the authenticated user's clinic ID.
    ///
    /// The ClinicMember authorization policy should validate
    /// this claim before the controller runs.
    /// </summary>
    public static int GetRequiredClinicId(this ClaimsPrincipal user)
    {
        string? clinicIdValue = user.FindFirst(CustomClaimTypes.ClinicId)?.Value;

        if (!int.TryParse(clinicIdValue, out int clinicId) || clinicId <= 0)
        {
            throw new InvalidOperationException(
                "The authenticated user does not have a valid ClinicId claim."
            );
        }

        return clinicId;
    }
}

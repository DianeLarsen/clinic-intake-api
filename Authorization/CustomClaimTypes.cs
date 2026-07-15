namespace ClinicIntakeApi.Authorization;

/// <summary>
/// Contains the custom claim names used by the API.
/// </summary>
public static class CustomClaimTypes
{
    // Identifies the clinic that owns the authenticated user.
    public const string ClinicId = "ClinicId";
}

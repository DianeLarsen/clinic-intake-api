namespace ClinicIntakeApi.Configuration;

/// <summary>
/// Represents pagination settings loaded from configuration.
///
/// ASP.NET fills these properties using the "Pagination"
/// section in appsettings.json.
/// </summary>
public class PaginationOptions
{
    // Store the section name in one place.
    //
    // This must match:
    //
    // "Pagination": { ... }
    //
    // in appsettings.json.
    public const string SectionName = "Pagination";

    // Number of results returned when the client
    // does not provide a pageSize query parameter.
    public int DefaultPageSize { get; set; }

    // Largest page size a client is allowed to request.
    public int MaximumPageSize { get; set; }
}

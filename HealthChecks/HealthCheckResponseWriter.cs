using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClinicIntakeApi.HealthChecks;

/// <summary>
/// Converts an ASP.NET health report into a safe JSON response.
/// </summary>
public static class HealthCheckResponseWriter
{
    public static Task WriteResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            // Overall result: Healthy, Degraded, or Unhealthy.
            status = report.Status.ToString(),

            // Show the result of each check without exposing
            // exceptions or sensitive configuration.
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),

                durationMilliseconds = Math.Round(entry.Value.Duration.TotalMilliseconds, 2),
            }),

            totalDurationMilliseconds = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
        };

        return context.Response.WriteAsJsonAsync(
            response,
            cancellationToken: context.RequestAborted
        );
    }
}

# Health Checks in ASP.NET Core

## What Is a Health Check?

A health check is a small endpoint used by people, monitoring systems, and hosting platforms to determine whether an application is operating correctly.

Health checks do not replace application tests. They answer a narrow question about the application right now:

> Can this running application safely receive work?

## Real-World Analogy

Think of the API as a medical device. A liveness check confirms that the device has power and its processor is responding. A readiness check performs a more useful self-test: it confirms that the device can also communicate with an important dependency. A glowing power light does not prove the entire system is ready for a patient, just as an HTTP server returning a hard-coded `Healthy` message does not prove its database works.

## Why the Old Endpoint Was Not Enough

The original health action always returned:

```json
{
  "status": "Healthy"
}
```

It returned the same answer even if the database could not be reached.

```text
Database works    → Healthy
Database is gone  → Still Healthy
```

That endpoint proved only that its controller action could return a hard-coded string.

ASP.NET health checks perform real checks and calculate an overall status.

## Liveness Versus Readiness

### Liveness

Liveness asks:

> Is the API process alive and able to respond?

This project exposes:

```http
GET /health/live
```

It does not test external dependencies. If the endpoint responds, the process is alive.

### Readiness

Readiness asks:

> Is the API ready to perform its normal work?

This project exposes:

```http
GET /health/ready
```

It runs the database health check. If the API cannot communicate with its database, it is alive but not ready.

```text
API responds + database works
    → Ready

API responds + database fails
    → Alive but not ready
```

## Why Health URLs Are Not Versioned

The health endpoints are:

```text
/health/live
/health/ready
```

They are infrastructure endpoints rather than public business API contracts.

Azure or another hosting platform should not need a new probe URL when the application adds `/api/v2`.

## Install the EF Core Health-Check Package

The project targets .NET 8, so it uses the matching package:

```bash
dotnet add package \
  Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore \
  --version 8.0.28
```

This package supplies:

```csharp
AddDbContextCheck<TContext>()
```

## Register the Database Check

Register the check after registering `ClinicIntakeDbContext`:

```csharp
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<ClinicIntakeDbContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]
    );
```

By default, the EF Core check calls:

```csharp
CanConnectAsync()
```

It verifies that the configured `DbContext` can communicate with the database.

The registration contains three important values:

| Value | Purpose |
| --- | --- |
| `database` | Human-readable check name |
| `Unhealthy` | Result used when the check fails |
| `ready` | Tag used to select the check for readiness |

## Why Use Tags?

Tags group health checks.

The database check is tagged:

```text
ready
```

The readiness endpoint selects checks with that tag:

```csharp
Predicate = check =>
    check.Tags.Contains("ready")
```

Later, other required dependencies could use the same tag:

```text
database       → ready
message queue  → ready
storage        → ready
```

The liveness endpoint deliberately selects no registered checks.

## Map the Liveness Endpoint

```csharp
app.MapHealthChecks(
        "/health/live",
        new HealthCheckOptions
        {
            // Select no dependency checks.
            Predicate = _ => false,

            ResponseWriter =
                HealthCheckResponseWriter.WriteResponseAsync,
        }
    )
    .AllowAnonymous();
```

The predicate returns `false` for every registered check. An empty health report is Healthy if the API process can produce it.

## Map the Readiness Endpoint

```csharp
app.MapHealthChecks(
        "/health/ready",
        new HealthCheckOptions
        {
            // Run checks tagged "ready".
            Predicate = check =>
                check.Tags.Contains("ready"),

            ResponseWriter =
                HealthCheckResponseWriter.WriteResponseAsync,
        }
    )
    .AllowAnonymous();
```

This endpoint currently selects the database check.

## Why Are Health Checks Anonymous?

Hosting platforms and load balancers must call health endpoints automatically. Requiring a user JWT would force infrastructure to sign in as a clinic user merely to determine whether the process is alive.

The call:

```csharp
.AllowAnonymous()
```

allows the probes to run without authentication.

Because the endpoints are public, their responses must not reveal:

- Connection strings
- Database paths
- Exception messages
- Stack traces
- Tokens or keys
- Patient information

## Structured JSON Responses

The default health-check response is plain text:

```text
Healthy
```

This project uses a response writer to return safe JSON with individual check results and timing.

Create `HealthChecks/HealthCheckResponseWriter.cs`:

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClinicIntakeApi.HealthChecks;

public static class HealthCheckResponseWriter
{
    public static Task WriteResponseAsync(
        HttpContext context,
        HealthReport report
    )
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),

            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),

                durationMilliseconds = Math.Round(
                    entry.Value.Duration.TotalMilliseconds,
                    2
                ),
            }),

            totalDurationMilliseconds = Math.Round(
                report.TotalDuration.TotalMilliseconds,
                2
            ),
        };

        return context.Response.WriteAsJsonAsync(
            response,
            cancellationToken: context.RequestAborted
        );
    }
}
```

The writer deliberately omits exception and configuration details.

## Example Liveness Response

```json
{
  "status": "Healthy",
  "checks": [],
  "totalDurationMilliseconds": 0
}
```

The empty `checks` array is intentional. Liveness tests only the process.

## Example Readiness Response

```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "durationMilliseconds": 1.23
    }
  ],
  "totalDurationMilliseconds": 1.23
}
```

Timing values vary between requests.

## Health Status Values

ASP.NET supports three health states:

### Healthy

The application or dependency is working normally.

### Degraded

The application can operate, but something is impaired.

### Unhealthy

The application should not be treated as ready to receive work.

The default HTTP mapping is:

| Health status | HTTP result |
| --- | --- |
| Healthy | `200 OK` |
| Degraded | `200 OK` |
| Unhealthy | `503 Service Unavailable` |

## Liveness Integration Test

```csharp
[Fact]
public async Task GetLiveness_WithoutToken_ReturnsHealthy()
{
    using HttpClient client = _factory.CreateClient();

    HttpResponseMessage response =
        await client.GetAsync("/health/live");

    string responseBody =
        await response.Content.ReadAsStringAsync();

    using JsonDocument json =
        JsonDocument.Parse(responseBody);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal(
        "Healthy",
        json.RootElement
            .GetProperty("status")
            .GetString()
    );

    Assert.Equal(
        0,
        json.RootElement
            .GetProperty("checks")
            .GetArrayLength()
    );
}
```

This proves the endpoint is public, responsive, and does not run dependency checks.

## Readiness Integration Test

```csharp
[Fact]
public async Task GetReadiness_WhenDatabaseIsAvailable_ReturnsHealthy()
{
    using HttpClient client = _factory.CreateClient();

    HttpResponseMessage response =
        await client.GetAsync("/health/ready");

    string responseBody =
        await response.Content.ReadAsStringAsync();

    using JsonDocument json =
        JsonDocument.Parse(responseBody);

    JsonElement checks =
        json.RootElement.GetProperty("checks");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal(1, checks.GetArrayLength());
    Assert.Equal(
        "database",
        checks[0].GetProperty("name").GetString()
    );
    Assert.Equal(
        "Healthy",
        checks[0].GetProperty("status").GetString()
    );
}
```

This proves the readiness endpoint runs the registered database check.

## Testing Failure Without Breaking the Real Database

The failure test creates a special test server and adds one test-only unhealthy check:

```csharp
using var unhealthyFactory =
    _factory.WithWebHostBuilder(builder =>
    {
        builder.ConfigureTestServices(services =>
        {
            services
                .AddHealthChecks()
                .AddCheck(
                    "forced-failure",
                    () => HealthCheckResult.Unhealthy(
                        "Forced failure for testing."
                    ),
                    tags: ["ready"]
                );
        });
    });
```

The test then verifies:

```csharp
Assert.Equal(
    HttpStatusCode.ServiceUnavailable,
    response.StatusCode
);

Assert.Equal(
    "Unhealthy",
    root.GetProperty("status").GetString()
);
```

The forced check exists only inside that test server. Production remains unchanged.

## How Azure Uses the Result

A hosting platform can call the readiness endpoint repeatedly.

```text
200 OK
    → Application is ready

503 Service Unavailable
    → Do not treat this instance as ready
```

Depending on platform configuration, failed probes can stop traffic from reaching the instance or trigger a restart.

## Avoid Expensive Health Checks

Health probes may run frequently. A database health check should be fast and lightweight.

The EF Core check uses `CanConnectAsync()` by default. Do not run a large patient query, full table scan, or complex report merely to prove connectivity. A health check that damages the system while checking it would be technically memorable but operationally unhelpful.

## Key Points

```text
Liveness
    → Is the process responding?

Readiness
    → Can the application perform required work?

Tag
    → Groups checks for an endpoint

Healthy
    → 200 OK

Unhealthy
    → 503 Service Unavailable

Public response
    → Must not expose sensitive diagnostics
```

## Reference

- [Health checks in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [EF Core health-check package](https://www.nuget.org/packages/Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore/8.0.28)
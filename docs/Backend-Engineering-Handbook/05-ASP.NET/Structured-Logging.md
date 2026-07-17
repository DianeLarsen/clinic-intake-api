# Structured Logging

## Problem

The API originally used `Console.WriteLine()` and separate request-start and request-end messages.

Those messages were useful while learning middleware order, but they had limitations:

- They were difficult to filter and search.
- They did not provide a consistent request summary.
- They did not record request duration or a trace identifier.
- They could not be easily sent to a centralized logging system.

The API needs useful logs for normal request outcomes, expected failures, and unexpected exceptions.

---

## Solution

Use ASP.NET Core's built-in `ILogger<T>` with structured message templates.

The application records:

- One completed-request log for every HTTP request
- The HTTP method and path
- Response status code
- Request duration
- Trace identifier
- Create, update, and delete outcomes
- Unexpected exceptions with the exception object and stack trace

---

## What Is Structured Logging?

Structured logging uses named placeholders rather than building a completed string.

Avoid string interpolation:

```csharp
_logger.LogInformation(
    $"Created intake request {request.Id}"
);
```

Use a message template instead:

```csharp
_logger.LogInformation(
    "Created intake request {RequestId} for patient {PatientId} in clinic {ClinicId}",
    request.Id,
    request.PatientId,
    request.ClinicId
);
```

`RequestId`, `PatientId`, and `ClinicId` are stored as separate fields.

A centralized log system can later filter records by values such as:

```text
ClinicId = 1
RequestId = 42
```

This is more useful than searching through a large pile of plain text logs.

---

## Request Logging Middleware

`RequestLoggingMiddleware` surrounds the rest of the request pipeline.

```csharp
public async Task InvokeAsync(HttpContext context)
{
    Stopwatch stopwatch = Stopwatch.StartNew();

    try
    {
        await _next(context);
    }
    finally
    {
        stopwatch.Stop();

        double elapsedMilliseconds = Math.Round(
            stopwatch.Elapsed.TotalMilliseconds,
            2
        );

        _logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms with trace {TraceIdentifier}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMilliseconds,
            context.TraceIdentifier
        );
    }
}
```

The `finally` block runs whether the request succeeds or an exception occurs downstream.

Example successful-request log:

```text
HTTP GET /health/live responded 200 in 21.08 ms with trace 0HNN44D5D6BH6:00000001
```

---

## Why Log One Request Summary?

The earlier approach wrote separate messages when a request entered and left middleware.

```text
--> GET /api/v1/requests
<-- 200
```

The completed-request log replaces both messages because it includes all of their useful information plus duration and trace ID.

One complete summary per request makes terminal output easier to read and avoids duplicate logs.

---

## Controller Outcome Logs

`RequestsController` records important state changes and expected outcomes.

Examples include:

```csharp
_logger.LogInformation(
    "Could not create intake request because patient {PatientId} was not found for clinic {ClinicId}",
    dto.PatientId,
    clinicId
);
```

```csharp
_logger.LogInformation(
    "Deleted intake request {RequestId} from clinic {ClinicId}",
    id,
    clinicId
);
```

Expected outcomes, including a missing request or patient, use `Information`.

They are not application errors. A client can request an ID that does not exist without the server having caught fire.

---

## Exception Logging and Trace IDs

`ExceptionHandlingMiddleware` logs unexpected exceptions at `Error` level.

```csharp
_logger.LogError(
    ex,
    "An unexpected error occurred while processing {Method} {Path}",
    context.Request.Method,
    context.Request.Path
);
```

For an unexpected failure, two logs have different jobs:

| Log | Purpose |
| --- | --- |
| `Error` exception log | Records the exception and stack trace for diagnosis. |
| `Information` request log | Records the final HTTP response, duration, and trace ID. |

Example:

```text
An unexpected error occurred while processing GET /api/v1/requests/error
System.Exception: This is a test exception.
```

```text
HTTP GET /api/v1/requests/error responded 500 in 52.52 ms with trace 0HNN44D5D6BH7:00000001
```

The trace ID helps connect entries created while processing the same request.

The API returns a safe generic response to the client instead of exposing stack traces or internal file paths.

```json
{
  "message": "An unexpected error occurred."
}
```

---

## Log-Level Configuration

`appsettings.json` controls the minimum log levels.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

This configuration means:

- Application `Information`, `Warning`, `Error`, and `Critical` logs are shown.
- `Trace` and `Debug` logs are hidden.
- Routine ASP.NET Core framework messages are hidden unless they are warnings or more severe.

This keeps useful application logs visible without flooding the terminal with framework chatter.

---

## Manual Verification

Successful request:

```bash
curl -i http://localhost:5090/health/live
```

Expected result:

```text
HTTP/1.1 200 OK
```

Unexpected-error test endpoint:

```bash
curl -i \
  -H "Authorization: Bearer <token>" \
  http://localhost:5090/api/v1/requests/error
```

Expected result:

```text
HTTP/1.1 500 Internal Server Error
```

Verification also included:

```bash
dotnet test
git diff --check
```

All 44 tests passed and the diff contained no whitespace errors.

---

## Key Takeaways

- Use `ILogger<T>` rather than `Console.WriteLine()`.
- Use named placeholders to create searchable structured fields.
- Log one complete request summary with status, duration, and trace ID.
- Use `Information` for normal application outcomes.
- Use `Error` for unexpected exceptions.
- Return safe error messages to clients while preserving diagnostic details in server logs.

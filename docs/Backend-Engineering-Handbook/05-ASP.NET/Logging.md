# Logging

## What is Logging?

Logging is the process of recording information about what an application is doing while it runs.

Logs help developers:

- Understand how requests move through the application
- Diagnose bugs
- Investigate failures
- Monitor performance
- Troubleshoot production issues

Without logging, debugging often becomes guesswork.

---

# Why Not Use Console.WriteLine()?

`Console.WriteLine()` works during development:

```csharp
Console.WriteLine("Request received.");
```

However, it has several limitations:

- No log levels
- No categories
- Difficult to filter
- Difficult to search
- Cannot easily send logs to external systems

ASP.NET provides a built-in logging system instead.

---

# ILogger<T>

ASP.NET uses the `ILogger<T>` interface.

Example:

```csharp
private readonly ILogger<RequestLoggingMiddleware> _logger;
```

The generic type (`<RequestLoggingMiddleware>`) tells ASP.NET which class created the log message.

ASP.NET automatically creates the logger using Dependency Injection.

---

# Injecting a Logger

A logger can be added to a constructor:

```csharp
public RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    _next = next;
    _logger = logger;
}
```

ASP.NET automatically provides the logger when creating the middleware.

---

# Log Levels

ASP.NET supports six log levels.

| Level | Purpose |
| --- | --- |
| Trace | Extremely detailed diagnostics |
| Debug | Developer debugging information |
| Information | Normal application behavior |
| Warning | Something unexpected happened |
| Error | An operation failed |
| Critical | The application may stop working |

---

# Trace

Used for very detailed diagnostic information.

Example:

```csharp
_logger.LogTrace(
    "Entering method GetRequests."
);
```

Typically disabled in production.

---

# Debug

Used while developing or debugging.

Example:

```csharp
_logger.LogDebug(
    "Loaded {Count} requests.",
    requestCount
);
```

Usually disabled in production.

---

# Information

Used for normal application behavior.

Example:

```csharp
_logger.LogInformation(
    "--> {Method} {Path}",
    context.Request.Method,
    context.Request.Path
);
```

Example output:

```text
info: ClinicIntakeApi.Middleware.RequestLoggingMiddleware[0]
      --> GET /api/v1/requests
```

---

# Warning

Used when something unexpected happens but the application can continue.

Example:

```csharp
_logger.LogWarning(
    "Patient {PatientId} was not found.",
    patientId
);
```

Examples:

- Missing data
- Invalid input
- Retry attempts

---

# Error

Used when an operation fails.

Example:

```csharp
_logger.LogError(
    ex,
    "An unexpected error occurred while processing {Method} {Path}",
    context.Request.Method,
    context.Request.Path
);
```

Examples:

- Exceptions
- Failed requests
- Database errors

---

# Critical

Used for severe failures.

Example:

```csharp
_logger.LogCritical(
    "The database connection failed."
);
```

Examples:

- Application startup failures
- Missing configuration
- Database unavailable

---

# Structured Logging

ASP.NET uses structured logging.

Instead of:

```csharp
_logger.LogInformation(
    $"GET {context.Request.Path}"
);
```

prefer:

```csharp
_logger.LogInformation(
    "--> {Method} {Path}",
    context.Request.Method,
    context.Request.Path
);
```

The placeholders:

```text
{Method}
{Path}
```

are stored separately from the message text.

This makes logs easier to search and analyze.

---

# Logging in Middleware

The API records one complete summary after each request finishes.

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

        _logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms with trace {TraceIdentifier}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2),
            context.TraceIdentifier
        );
    }
}
```

Example output:

```text
info: ClinicIntakeApi.Middleware.RequestLoggingMiddleware[0]
      HTTP GET /health/live responded 200 in 21.08 ms with trace 0HNN44D5D6BH6:00000001
```

The log includes the request method, path, response status, duration, and trace identifier in one entry.

---

# Logging Exceptions

Example:

```csharp
catch (Exception ex)
{
    _logger.LogError(
        ex,
        "An unexpected error occurred while processing {Method} {Path}",
        context.Request.Method,
        context.Request.Path
    );
}
```

Example output:

```text
fail: ClinicIntakeApi.Middleware.ExceptionHandlingMiddleware[0]

System.InvalidOperationException:
Something went wrong.
```

---

# Configuring Log Levels

Log levels are configured in:

```text
appsettings.json
```

Example:

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

---

# How Log Filtering Works

If the log level is:

```json
"Default": "Information"
```

ASP.NET shows:

- Information
- Warning
- Error
- Critical

It hides:

- Debug
- Trace

---

# Development vs Production

During development:

- Logs appear in the terminal.
- Detailed messages are useful.

During production:

- Logs are often sent to:
  - Files
  - Databases
  - Azure Application Insights
  - Monitoring systems

The application code usually stays the same.

Only the logging destination changes.

---

# Dependency Injection and Logging

ASP.NET creates loggers automatically.

```text
Dependency Injection
        ↓
ILogger<T>
        ↓
Controller / Service / Middleware
        ↓
Log message
```

Any class created by ASP.NET can request a logger.

---

# Request Flow with Logging

```text
HTTP Request
        ↓
Middleware
        ↓
ILogger
        ↓
Controller
        ↓
Service
        ↓
Repository
        ↓
Database
        ↓
HTTP Response
```

Logs provide visibility into each step of the request.

---

# Important Idea

Logging records what the application is doing while it runs.

```text
Application
        ↓
ILogger
        ↓
Terminal / File / Azure
```

Good logs make debugging easier and help developers understand failures without changing code.
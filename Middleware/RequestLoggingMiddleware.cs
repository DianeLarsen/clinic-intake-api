using System.Diagnostics;

namespace ClinicIntakeApi.Middleware;

//
// Custom middleware that logs every incoming request
// and outgoing response.
//
// Middleware sits in the ASP.NET request pipeline.
// Every request passes through this class before reaching
// the controller, and every response passes back through it
// before being sent to the client.
//
// Instead of writing directly to the console,
// this middleware uses ASP.NET's logging system.
//
public class RequestLoggingMiddleware
{
    //
    // Represents the next step in the request pipeline.
    //
    // This might be:
    //
    // • Another middleware
    // • ASP.NET routing
    // • Authentication
    // • Authorization
    // • Eventually, a controller
    //
    // Calling _next(context) tells ASP.NET:
    //
    // "I'm finished. Continue processing this request."
    //
    private readonly RequestDelegate _next;

    //
    // ASP.NET's built-in logger.
    //
    // ILogger<T> automatically labels log messages
    // with the class that created them.
    //
    // In this case, all messages will be associated with:
    //
    //     RequestLoggingMiddleware
    //
    // In development, logs appear in the terminal.
    //
    // In production, logs could be sent to:
    //
    // • Files
    // • Databases
    // • Azure Application Insights
    // • Monitoring systems
    //
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    //
    // Constructor
    //
    // ASP.NET's Dependency Injection container automatically
    // provides both:
    //
    // • The next middleware in the pipeline
    // • An ILogger<RequestLoggingMiddleware>
    //
    // The middleware stores both so they can be used later.
    //
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    //
    // InvokeAsync() is called once for every HTTP request.
    //
    // HttpContext contains information about:
    //
    // • The incoming request
    // • The outgoing response
    // • Headers
    // • Route values
    // • User information
    // • Services
    //
    public async Task InvokeAsync(HttpContext context)
    {
        //
        // Start timing before the request continues through
        // the rest of the middleware pipeline.
        //
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            //
            // Pass control to the next middleware.
            //
            // Execution pauses here until the rest of the
            // pipeline finishes processing the request.
            //
            await _next(context);
        }
        finally
        {
            //
            // finally runs whether the request succeeds or
            // an exception is handled farther down the pipeline.
            //
            stopwatch.Stop();

            //
            // Convert the elapsed time into milliseconds and
            // round it for easier reading in the log output.
            //
            double elapsedMilliseconds = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);

            //
            // Record one structured summary for the completed request.
            //
            // The named placeholders become searchable fields:
            //
            // {Method}
            // {Path}
            // {StatusCode}
            // {ElapsedMilliseconds}
            // {TraceIdentifier}
            //
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
}

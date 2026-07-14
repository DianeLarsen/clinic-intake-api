using System.Text.Json;

namespace ClinicIntakeApi.Middleware;

//
// This middleware catches unexpected exceptions
// that occur while processing an HTTP request.
//
// Without this middleware, an unhandled exception
// would stop the request and ASP.NET would return
// its default error response.
//
// Instead, this middleware:
//
// • Logs the exception
// • Returns HTTP 500
// • Sends a consistent JSON response
//
public class ExceptionHandlingMiddleware
{
    //
    // Represents the next middleware in the request pipeline.
    //
    // Calling _next(context) tells ASP.NET:
    //
    // "Continue processing this request."
    //
    private readonly RequestDelegate _next;

    //
    // ASP.NET's built-in logging service.
    //
    // ILogger<T> automatically tags log messages
    // with the name of the class that created them.
    //
    // In this case, logs will be associated with:
    //
    //     ExceptionHandlingMiddleware
    //
    // The logging system can later send messages to:
    //
    // • The terminal
    // • Files
    // • Databases
    // • Azure Application Insights
    // • Monitoring systems
    //
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    //
    // Constructor
    //
    // ASP.NET's Dependency Injection container
    // automatically provides:
    //
    // • The next middleware in the pipeline
    // • A logger
    //
    // The middleware stores both for later use.
    //
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    //
    // ASP.NET automatically calls InvokeAsync()
    // for every incoming HTTP request.
    //
    // HttpContext contains information about:
    //
    // • The request
    // • The response
    // • Headers
    // • The current user
    // • Route values
    //
    public async Task InvokeAsync(HttpContext context)
    {
        //
        // Try to continue processing the request.
        //
        // This sends the request farther down
        // the middleware pipeline.
        //
        try
        {
            await _next(context);
        }
        //
        // If ANY code later in the pipeline throws
        // an exception, execution immediately jumps here.
        //
        catch (Exception ex)
        {
            //
            // Log the exception.
            //
            // LogError() records:
            //
            // • The exception object
            // • The HTTP method
            // • The requested path
            //
            // Example:
            //
            // GET /api/v1/requests/123
            //
            // Structured logging stores the values
            // separately instead of building one large string.
            //
            _logger.LogError(
                ex,
                "An unexpected error occurred while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path
            );

            //
            // Tell the client that the server failed.
            //
            // HTTP 500 = Internal Server Error
            //
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            //
            // Tell the client that the response body
            // will contain JSON.
            //
            context.Response.ContentType = "application/json";

            //
            // Create an anonymous object that will
            // become JSON.
            //
            // This object:
            //
            // new { message = "An unexpected error occurred." }
            //
            // becomes:
            //
            // {
            //     "message": "An unexpected error occurred."
            // }
            //
            var response = new { message = "An unexpected error occurred." };

            //
            // Convert the object into JSON text
            // and send it back to the client.
            //
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

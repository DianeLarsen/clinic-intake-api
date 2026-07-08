namespace ClinicIntakeApi.Middleware;

//
// Custom middleware that logs every incoming request
// and the outgoing HTTP response.
//
// Middleware sits in the ASP.NET request pipeline.
// Every request passes through this class before reaching
// the controller, and every response passes back through it
// before being sent to the client.
//
public class RequestLoggingMiddleware
{
    //
    // Represents the next step in the request pipeline.
    //
    // This might be:
    // • Another middleware
    // • ASP.NET routing
    // • Authentication
    // • Authorization
    // • Eventually, a controller
    //
    // Calling _next(context) tells ASP.NET:
    // "I'm finished. Continue processing this request."
    //
    private readonly RequestDelegate _next;

    //
    // Constructor
    //
    // ASP.NET automatically passes the next middleware
    // into the constructor when building the pipeline.
    //
    // Each middleware stores a reference to the next one,
    // creating a chain of middleware components.
    //
    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    //
    // InvokeAsync() is the method ASP.NET calls
    // every time an HTTP request reaches this middleware.
    //
    // HttpContext contains everything about the current request,
    // including:
    //
    // • Request information
    // • Response information
    // • User information
    // • Headers
    // • Route values
    // • Services
    //
    public async Task InvokeAsync(HttpContext context)
    {
        //
        // This code executes BEFORE the controller.
        //
        // Log the HTTP method and requested URL.
        //
        // Example:
        //
        // --> GET /requests
        //
        Console.WriteLine("Before Controller");
        Console.WriteLine($"--> {context.Request.Method} {context.Request.Path}");

        //
        // Pass control to the next middleware
        // in the request pipeline.
        //
        // This eventually reaches the controller.
        //
        // Execution PAUSES here until the rest of the
        // pipeline has finished processing the request.
        //
        await _next(context);

        //
        // This code executes AFTER the controller
        // and the remaining middleware have completed.
        //
        // At this point ASP.NET has generated
        // the HTTP response.
        //
        // Example:
        //
        // <-- 200
        //
        Console.WriteLine($"<-- {context.Response.StatusCode}");
        Console.WriteLine("After Controller");
    }
}

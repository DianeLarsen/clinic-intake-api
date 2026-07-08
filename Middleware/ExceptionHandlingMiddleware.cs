using System.Text.Json;

namespace ClinicIntakeApi.Middleware;

//
// This middleware catches any unexpected exceptions
// that occur while processing an HTTP request.
//
// Without this middleware, an unhandled exception could
// crash the request and return a generic error page.
//
// Instead, we catch the exception here and return
// a consistent JSON error response.
//
public class ExceptionHandlingMiddleware
{
    //
    // Holds a reference to the next middleware
    // in the ASP.NET request pipeline.
    //
    // Think of this as:
    // "Who should I send the request to next?"
    //
    private readonly RequestDelegate _next;

    //
    // Constructor
    //
    // ASP.NET automatically passes the next middleware
    // into this constructor when building the pipeline.
    //
    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    //
    // ASP.NET automatically calls InvokeAsync()
    // every time an HTTP request arrives.
    //
    public async Task InvokeAsync(HttpContext context)
    {
        //
        // Try to continue processing the request.
        //
        // This sends the request to the next middleware,
        // and eventually to the controller.
        //
        try
        {
            await _next(context);
        }
        //
        // If ANY code later in the pipeline throws
        // an exception, execution jumps here.
        //
        catch (Exception ex)
        {
            //
            // Write the full exception to the console.
            //
            // During development this helps us see
            // exactly what went wrong.
            //
            Console.WriteLine(ex);

            //
            // Tell the client that something went wrong.
            //
            // HTTP 500 = Internal Server Error
            //
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            //
            // Tell the client the response will contain JSON.
            //
            context.Response.ContentType = "application/json";

            //
            // Create an object that will become JSON.
            //
            // This:
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

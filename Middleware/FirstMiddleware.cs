namespace ClinicIntakeApi.Middleware;

public class FirstMiddleware
{
    private readonly RequestDelegate _next;

    public FirstMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine("FirstMiddleware: before next");

        await _next(context);

        Console.WriteLine("FirstMiddleware: after next");
    }
}

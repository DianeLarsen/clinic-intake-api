namespace ClinicIntakeApi.Middleware;

public class SecondMiddleware
{
    private readonly RequestDelegate _next;

    public SecondMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine("SecondMiddleware: before next");

        await _next(context);

        Console.WriteLine("SecondMiddleware: after next");
    }
}

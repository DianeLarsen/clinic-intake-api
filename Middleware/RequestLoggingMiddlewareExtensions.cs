namespace ClinicIntakeApi.Middleware;

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }

    public static IApplicationBuilder UseFirstMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<FirstMiddleware>();
    }

    public static IApplicationBuilder UseSecondMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecondMiddleware>();
    }

    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

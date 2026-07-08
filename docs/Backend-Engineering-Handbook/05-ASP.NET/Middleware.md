# Middleware

## What Problem Does This Solve?

Every HTTP request entering an ASP.NET Core application follows the same path.

Many tasks need to happen before a request reaches a controller.

Examples include:

- Logging
- Authentication
- Authorization
- HTTPS redirection
- Exception handling
- CORS
- Response compression

Rather than placing this logic inside every controller, ASP.NET Core uses middleware.

## Solution

Middleware is code that executes before and after the next step in the request pipeline.

Each middleware component can:

- Inspect the request
- Modify the request
- Stop the request
- Continue the request
- Inspect the response
- Modify the response

## Why This Matters

Middleware allows cross-cutting concerns to be handled in one place instead of duplicated throughout the application.

## Mental Model

Think of middleware as checkpoints.

```text
Client
   │
   ▼
Middleware
   ▼
Middleware
   ▼
Middleware
   ▼
Controller
```

Every request passes through each middleware in order.

When the response is created, it travels back through the middleware in reverse order.

```text
Request
   │
   ▼
Middleware A
   ▼
Middleware B
   ▼
Controller
   ▲
Middleware B
   ▲
Middleware A
   ▲
Response
```

## Anatomy of Middleware

A custom middleware contains three important parts.

### RequestDelegate

```csharp
private readonly RequestDelegate _next;
```

`RequestDelegate` represents the next step in the request pipeline.

Calling:

```csharp
await _next(context);
```

passes control to the next middleware.

---

### HttpContext

```csharp
public async Task InvokeAsync(HttpContext context)
```

`HttpContext` contains information about the current request and response.

Examples include:

- Request path
- HTTP method
- Headers
- User
- Query string
- Response status code

---

### InvokeAsync()

ASP.NET calls `InvokeAsync()` once for every HTTP request.

## Example

```csharp
public async Task InvokeAsync(HttpContext context)
{
    Console.WriteLine(
        $"--> {context.Request.Method} {context.Request.Path}");

    await _next(context);

    Console.WriteLine(
        $"<-- {context.Response.StatusCode}");
}
```

Everything before `await _next(context)` runs before the controller.

Everything after runs after the controller.

## Registering Middleware

Middleware must be added to the pipeline.

```csharp
app.UseMiddleware<RequestLoggingMiddleware>();
```

Simply creating the class does nothing until it is registered.

## Middleware Order

Order matters.

ASP.NET executes middleware in the order it is registered.

```csharp
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();
```

Changing the order changes application behavior.

## Common Built-In Middleware

Examples include:

- HTTPS Redirection
- Authentication
- Authorization
- Static Files
- Routing
- CORS
- Response Compression

## Real-World Example

The Clinic Intake API uses custom logging middleware to display each incoming request and outgoing response.

Example output:

```text
--> GET /requests

<-- 200
```

## Common Beginner Questions

### Does middleware replace controllers?

No.

Middleware runs before controllers.

Controllers still handle application-specific HTTP requests.

---

### What happens if `_next(context)` is not called?

The request stops.

The controller never executes.

---

### Can middleware modify responses?

Yes.

Since middleware executes again after the controller returns, it can inspect or modify the response before it is sent to the client.

## Common Mistakes

- Forgetting to register middleware.
- Forgetting `await _next(context)`.
- Registering middleware in the wrong order.
- Putting business logic into middleware.

## Interview Answer

Middleware is software that executes during the ASP.NET Core request pipeline. Each middleware component can inspect or modify requests and responses before passing control to the next middleware using `RequestDelegate`.

## One-Sentence Summary

Middleware processes HTTP requests and responses before and after the rest of the ASP.NET request pipeline.

## What Finally Made It Click

Middleware is not part of the controller.

It surrounds the controller.

Every request passes through middleware on the way in, and every response passes back through the same middleware on the way out.

## Related Topics

### Previous

- Controllers
- Program.cs

### Next

- Request Lifecycle
- Authentication
- Exception Handling
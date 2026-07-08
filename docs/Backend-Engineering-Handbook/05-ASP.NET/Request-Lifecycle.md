# Request Lifecycle

## What Problem Does This Solve?

An ASP.NET Core application has many moving parts.

A request may pass through:

- Kestrel
- Middleware
- Routing
- Controllers
- Services
- Repositories
- Entity Framework Core
- The database

Without a request lifecycle mental model, these pieces can feel disconnected.

The request lifecycle explains what happens from the moment an HTTP request reaches the application until the HTTP response is sent back to the client.

## Solution

Think of the request lifecycle as the path one HTTP request takes through the application.

Example:

```http
GET /requests/5
```

That request enters ASP.NET Core, moves through the pipeline, reaches the correct controller action, calls the application layers, gets data from the database, and returns a response.

## Why This Matters

Understanding the request lifecycle helps explain:

- Where middleware runs
- Where controllers fit
- How Dependency Injection creates objects
- When services and repositories execute
- Where Entity Framework queries happen
- How HTTP responses are created
- Why middleware order matters

Once the lifecycle makes sense, ASP.NET Core stops feeling like magic and starts looking like a very organized conveyor belt. An expensive conveyor belt, naturally.

## High-Level Diagram

```text
Client / Swagger
        │
        ▼
Kestrel Web Server
        │
        ▼
Middleware
        │
        ▼
Routing
        │
        ▼
Controller
        │
        ▼
Service Layer
        │
        ▼
Repository Layer
        │
        ▼
Entity Framework Core
        │
        ▼
SQLite Database
        │
        ▼
Entity Framework Core
        │
        ▼
Repository Layer
        │
        ▼
Service Layer
        │
        ▼
Controller
        │
        ▼
Middleware
        │
        ▼
HTTP Response
        │
        ▼
Client / Swagger
```

## Example Request

Suppose Swagger sends:

```http
GET /requests/5
```

The matching controller action is:

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    IntakeRequest? request =
        await _intakeService.FindRequestByIdAsync(id);

    return request is not null
        ? Ok(request)
        : NotFound();
}
```

## Step-by-Step Walkthrough

### Step 1: The Application Is Running

The application starts in `Program.cs`.

```csharp
var app = builder.Build();

app.Run();
```

`app.Run()` starts the web application and waits for incoming HTTP requests.

Nothing interesting happens until a client sends a request.

---

### Step 2: The Client Sends a Request

A client sends:

```http
GET /requests/5
```

The client could be:

- Swagger
- A browser
- Postman
- A frontend application
- Another API

---

### Step 3: Kestrel Receives the Request

Kestrel is ASP.NET Core's built-in web server.

It receives the HTTP request and passes it into the ASP.NET Core pipeline.

Mental model:

```text
Client
   │
   ▼
Kestrel
```

Kestrel is like the receptionist. It accepts the request and sends it to the right internal process. Very glamorous. Probably underpaid.

---

### Step 4: Middleware Runs

Middleware runs before the request reaches the controller.

Example:

```csharp
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
```

Middleware can:

- Log the request
- Redirect HTTP to HTTPS
- Authenticate the user
- Authorize access
- Handle exceptions
- Stop the request
- Pass the request forward

Custom middleware example:

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

Everything before:

```csharp
await _next(context);
```

runs before the controller.

Everything after it runs after the controller.

---

### Step 5: Routing Selects the Controller Action

ASP.NET looks at the request path and HTTP method.

Request:

```http
GET /requests/5
```

Controller route:

```csharp
[Route("[controller]")]
public class RequestsController : ControllerBase
```

Action route:

```csharp
[HttpGet("{id}")]
```

ASP.NET combines them:

```text
/requests + /{id}
```

Result:

```http
GET /requests/5
```

So ASP.NET selects:

```csharp
RequestsController.GetById(int id)
```

---

### Step 6: Dependency Injection Creates the Controller

The controller asks for a service:

```csharp
public RequestsController(IIntakeService intakeService)
{
    _intakeService = intakeService;
}
```

ASP.NET uses the DI container to build the dependency chain.

```text
RequestsController
        │
        ▼
IntakeService
        │
        ▼
EfIntakeRepository
        │
        ▼
ClinicIntakeDbContext
```

These registrations come from `Program.cs`:

```csharp
builder.Services.AddScoped<IIntakeRepository, EfIntakeRepository>();

builder.Services.AddScoped<IIntakeService, IntakeService>();
```

`DbContext` is also registered:

```csharp
builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite("Data Source=clinic-intake.db"));
```

---

### Step 7: The Controller Runs

The controller action executes:

```csharp
IntakeRequest? request =
    await _intakeService.FindRequestByIdAsync(id);
```

The controller does not query the database directly.

It delegates work to the Service Layer.

---

### Step 8: The Service Layer Runs

The service contains business/application logic.

Example:

```csharp
public async Task<IntakeRequest?> FindRequestByIdAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}
```

In this case, the service delegates to the repository.

In more complex applications, the service might enforce business rules before calling the repository.

---

### Step 9: The Repository Runs

The repository handles data access.

Example:

```csharp
public async Task<IntakeRequest?> GetByIdAsync(int id)
{
    return await _db.IntakeRequests
        .FirstOrDefaultAsync(r => r.Id == id);
}
```

The repository knows about Entity Framework Core.

The controller does not.

---

### Step 10: Entity Framework Generates SQL

The repository writes LINQ:

```csharp
_db.IntakeRequests
    .FirstOrDefaultAsync(r => r.Id == id);
```

Entity Framework Core translates that LINQ query into SQL.

The developer writes C#.

EF Core generates SQL.

A rare moment where letting a framework do the tedious work is not a terrible idea.

---

### Step 11: SQLite Executes the Query

SQLite receives the SQL, searches the database, and returns the matching row.

Example:

```text
Request #5
```

If the row exists, SQLite returns data.

If it does not exist, the result is `null`.

---

### Step 12: EF Core Creates C# Objects

Entity Framework takes the database result and creates C# objects.

Example:

```csharp
IntakeRequest
```

If tracking is enabled, EF Core also begins tracking the entity.

---

### Step 13: Data Returns Through the Layers

The data flows back upward.

```text
SQLite
   ▲
Entity Framework Core
   ▲
Repository
   ▲
Service
   ▲
Controller
```

Each layer returns the result to the layer above it.

---

### Step 14: Controller Creates the HTTP Response

The controller decides which HTTP response to return.

```csharp
return request is not null
    ? Ok(request)
    : NotFound();
```

If the request exists:

```http
200 OK
```

If it does not exist:

```http
404 Not Found
```

---

### Step 15: ASP.NET Serializes the Response

ASP.NET converts the C# object into JSON.

Example:

```json
{
  "id": 5,
  "clinicId": 1,
  "patientId": 3,
  "status": "Submitted"
}
```

The JSON settings configured in `Program.cs` affect this step.

Example:

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});
```

This allows enum values to appear as strings instead of numbers.

---

### Step 16: Response Travels Back Through Middleware

After the controller creates the response, control returns back through middleware.

In custom middleware:

```csharp
await _next(context);

Console.WriteLine(
    $"<-- {context.Response.StatusCode}");
```

The response status code can now be logged.

---

### Step 17: The Client Receives the Response

The client receives:

```http
200 OK
```

or:

```http
404 Not Found
```

The request lifecycle is complete.

## Request and Response Flow

Request path:

```text
Client
   │
   ▼
Kestrel
   ▼
Middleware
   ▼
Routing
   ▼
Controller
   ▼
Service
   ▼
Repository
   ▼
Entity Framework
   ▼
SQLite
```

Response path:

```text
SQLite
   ▲
Entity Framework
   ▲
Repository
   ▲
Service
   ▲
Controller
   ▲
Middleware
   ▲
Kestrel
   ▲
Client
```

## Where ASP.NET Features Fit

### Middleware

Runs before and after controllers.

Examples:

- Logging
- HTTPS redirection
- Exception handling
- Authentication
- Authorization

---

### Routing

Chooses the controller action.

Example:

```csharp
[HttpGet("{id}")]
```

---

### Controllers

Handle HTTP-specific work.

Examples:

- Read route parameters
- Read query parameters
- Accept DTOs
- Return HTTP responses

---

### Services

Handle business/application logic.

---

### Repositories

Handle data access.

---

### Entity Framework Core

Translates LINQ into SQL and maps database rows to C# objects.

---

### Database

Stores and retrieves data.

## Common Beginner Questions

### Is the controller the whole application?

No.

The controller is only one stop in the request lifecycle.

It receives the HTTP request after middleware and routing, calls the service layer, and returns an HTTP response.

---

### Does middleware run before controllers?

Yes.

Middleware runs before the controller on the way in and after the controller on the way out.

---

### When does Dependency Injection happen?

ASP.NET uses Dependency Injection when it creates controllers and their dependencies.

For example, it supplies `IIntakeService` to `RequestsController`.

---

### When does Entity Framework run?

Entity Framework runs when repository methods execute LINQ queries or save changes.

---

### Why does the response go back through middleware?

Because middleware surrounds the rest of the pipeline.

Code after:

```csharp
await _next(context);
```

runs after the controller has completed.

## Common Mistakes

- Thinking controllers are the whole application.
- Forgetting that middleware runs before controllers.
- Forgetting that response middleware runs after controllers.
- Putting business logic in controllers.
- Accessing `DbContext` directly from controllers.
- Not understanding that middleware order changes behavior.
- Forgetting that Dependency Injection builds the controller and service chain.

## Interview Answer

The ASP.NET Core request lifecycle describes how an HTTP request flows through the application. Kestrel receives the request, middleware processes it, routing selects the correct controller action, Dependency Injection creates the controller and its dependencies, the controller calls services and repositories, Entity Framework queries the database, and the response travels back through the pipeline to the client.

## One-Sentence Summary

The request lifecycle is the path an HTTP request takes through ASP.NET Core from Kestrel, through middleware and controllers, down to the database, and back as an HTTP response.

## What Finally Made It Click

- The controller is not the whole application.
- Middleware runs before and after controllers.
- Routing decides which controller action handles the request.
- Dependency Injection builds the controller and service chain.
- Services handle application logic.
- Repositories handle data access.
- Entity Framework translates C# LINQ into SQL.
- The response travels back through the same pipeline.
- Every ASP.NET feature fits somewhere in the lifecycle.

## Related Topics

### Previous

- Program.cs
- Controllers
- Middleware

### Next

- API Versioning
- Exception Handling
- Logging

### See Also

- Dependency Injection
- Service Layer
- Repository Pattern
- Entity Framework Core
- API Responses
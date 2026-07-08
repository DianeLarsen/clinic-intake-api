# Controllers

## What Problem Does This Solve?

As an API grows, keeping every endpoint inside `Program.cs` becomes difficult to manage.

Minimal APIs work well for small applications, but larger projects often need a clearer way to organize endpoints by resource.

For example:

```text
Requests
Patients
Clinics
Doctors
Appointments
Reports
```

Without controllers, `Program.cs` can become a giant file containing every route in the application. Naturally, humanity saw one messy file and invented classes. Occasionally, we make progress.

## Solution

Controllers organize related endpoints into classes.

Instead of this:

```csharp
app.MapGet("/requests", ...);

app.MapPost("/requests", ...);

app.MapPut("/requests/{id}/status", ...);

app.MapDelete("/requests/{id}", ...);
```

the endpoints move into a controller:

```csharp
[ApiController]
[Route("[controller]")]
public class RequestsController : ControllerBase
{
    [HttpGet]

    [HttpPost]

    [HttpPut("{id}/status")]

    [HttpDelete("{id}")]
}
```

Each controller usually represents one resource.

## Why This Matters

Controllers make APIs easier to organize, maintain, and expand.

They provide:

- Resource-based endpoint organization
- Attribute-based routing
- Constructor dependency injection
- Built-in response helpers
- Automatic API behaviors with `[ApiController]`
- A familiar structure used in many ASP.NET Core applications

Controllers also help keep `Program.cs` focused on application setup instead of endpoint definitions.

## Mental Model

A controller is a class that groups related HTTP actions.

```text
HTTP Request
      │
      ▼
Routing
      │
      ▼
RequestsController
      │
      ▼
Service Layer
      │
      ▼
Repository
      │
      ▼
Database
```

The controller handles the HTTP boundary.

The service handles business logic.

The repository handles data access.

## Controller Structure

Example:

```csharp
using ClinicIntakeApi.Dtos;
using ClinicIntakeApi.Models;
using ClinicIntakeApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicIntakeApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RequestsController : ControllerBase
{
    private readonly IIntakeService _intakeService;

    public RequestsController(IIntakeService intakeService)
    {
        _intakeService = intakeService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var requests =
            await _intakeService.GetRequestSummariesAsync(
                null,
                null,
                null,
                1,
                10);

        return Ok(requests);
    }
}
```

## Key Parts

### `[ApiController]`

Marks the class as an API controller.

```csharp
[ApiController]
```

This enables API-specific behavior such as:

- Improved model binding
- Automatic validation responses
- Better error responses
- Required parameter inference

---

### `[Route("[controller]")]`

Defines the base route for the controller.

```csharp
[Route("[controller]")]
```

For a class named:

```csharp
RequestsController
```

ASP.NET removes `Controller` and creates the base route:

```http
/requests
```

---

### `ControllerBase`

Controllers inherit from `ControllerBase`.

```csharp
public class RequestsController : ControllerBase
```

This gives access to helper methods such as:

```csharp
Ok()

NotFound()

Created()

BadRequest()

NoContent()
```

These create HTTP responses.

---

### Constructor Dependency Injection

Controllers receive services through their constructor.

```csharp
private readonly IIntakeService _intakeService;

public RequestsController(IIntakeService intakeService)
{
    _intakeService = intakeService;
}
```

ASP.NET creates the controller and automatically supplies the registered service.

This replaces the Minimal API pattern of injecting services directly into each endpoint.

## HTTP Attributes

Controllers use attributes to define endpoints.

### GET All

```csharp
[HttpGet]
public async Task<IActionResult> Get()
```

Route:

```http
GET /requests
```

---

### GET by ID

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
```

Route:

```http
GET /requests/5
```

---

### POST

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateRequestDto dto)
```

Route:

```http
POST /requests
```

---

### PUT

```csharp
[HttpPut("{id}/status")]
public async Task<IActionResult> UpdateStatus(
    int id,
    UpdateRequestStatusDto dto)
```

Route:

```http
PUT /requests/5/status
```

---

### DELETE

```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
```

Route:

```http
DELETE /requests/5
```

## Minimal APIs vs Controllers

### Minimal API

```csharp
app.MapGet(
    "/requests",
    async (IIntakeService intakeService) =>
    {
        return Results.Ok(
            await intakeService.GetRequestSummariesAsync(
                null,
                null,
                null,
                1,
                10));
    });
```

### Controller

```csharp
[HttpGet]
public async Task<IActionResult> Get()
{
    var requests =
        await _intakeService.GetRequestSummariesAsync(
            null,
            null,
            null,
            1,
            10);

    return Ok(requests);
}
```

Both approaches can produce the same API behavior.

The difference is organization.

Minimal APIs define routes directly in `Program.cs`.

Controllers organize routes into classes.

## Registering Controllers

Controllers must be registered in `Program.cs`.

```csharp
builder.Services.AddControllers();
```

Then mapped after the app is built:

```csharp
app.MapControllers();
```

Without these lines, ASP.NET will not discover or route controller actions. The controller can sit there beautifully written and completely ignored, like most documentation.

## Real-World Example

The Clinic Intake API was first built with Minimal APIs.

The endpoints were later converted into a controller:

```text
RequestsController
    ├── GET /requests
    ├── GET /requests/{id}
    ├── POST /requests
    ├── PUT /requests/{id}/status
    └── DELETE /requests/{id}
```

The Service Layer, Repository Layer, and Entity Framework code did not need to change.

Only the HTTP entry point changed.

That proved the architecture was separated correctly.

## Common Beginner Questions

### Are Controllers better than Minimal APIs?

Not always.

Minimal APIs are great for small APIs, microservices, prototypes, and simple endpoints.

Controllers are often better for larger APIs with many related endpoints.

---

### Do Controllers replace Services?

No.

Controllers should not contain business logic.

They should call services.

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
Database
```

---

### Why return `IActionResult`?

`IActionResult` allows an action to return different HTTP responses.

Example:

```csharp
return request is not null
    ? Ok(request)
    : NotFound();
```

The same method can return `200 OK` or `404 Not Found`.

---

### Do Controllers still use DTOs?

Yes.

Controllers are the boundary between HTTP and the application.

They should receive request DTOs and return response DTOs.

---

### What happened to endpoint filters?

Controllers often use `[ApiController]` and model validation instead of Minimal API endpoint filters.

Endpoint filters are still useful in Minimal APIs.

## Common Mistakes

- Putting business logic inside controllers.
- Forgetting `builder.Services.AddControllers()`.
- Forgetting `app.MapControllers()`.
- Leaving old Minimal API routes active and creating duplicate routes.
- Returning `Ok(true)` for updates instead of `NoContent()`.
- Returning EF entities directly when a DTO would be safer.
- Making `Program.cs` responsible for too much.

## Interview Answer

Controllers in ASP.NET Core organize related API endpoints into classes. They use attributes such as `[HttpGet]`, `[HttpPost]`, and `[Route]` to define routes and inherit from `ControllerBase` to return HTTP responses. Controllers are commonly used in larger APIs to keep endpoints organized while delegating business logic to services.

## One-Sentence Summary

Controllers organize related HTTP endpoints into classes while keeping business logic in the service layer.

## What Finally Made It Click

- A Controller is just another way to define endpoints.
- Minimal APIs put endpoints in `Program.cs`.
- Controllers put endpoints in classes.
- The Service Layer, Repository Layer, and EF Core code do not need to change when switching from Minimal APIs to Controllers.
- `[Route("[controller]")]` uses the controller class name to build the base URL.
- `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, and `[HttpDelete]` replace `app.MapGet()`, `app.MapPost()`, `app.MapPut()`, and `app.MapDelete()`.
- Controllers should be thin. They receive HTTP input, call the service, and return HTTP responses.

## Related Topics

### Previous

- Minimal APIs
- REST APIs
- Program.cs

### Next

- Middleware
- Request Lifecycle
- Model Binding

### See Also

- Dependency Injection
- DTOs
- Service Layer
- API Responses
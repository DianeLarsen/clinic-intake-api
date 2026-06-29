````markdown
# Minimal APIs

## What Problem Does This Solve?

Applications need a way to expose functionality over HTTP.

Traditionally, ASP.NET used controllers, which require multiple files, attributes, and classes before an endpoint can respond to a request.

For small APIs or microservices, this adds unnecessary complexity.

## Solution

Minimal APIs allow HTTP endpoints to be defined directly in `Program.cs` using simple methods such as:

- `MapGet()`
- `MapPost()`
- `MapPut()`
- `MapDelete()`

Instead of creating a controller, the endpoint and its logic can be written in one place.

Example:

```csharp
app.MapGet("/requests", () =>
{
    return Results.Ok();
});
```

## Where It Fits

Minimal APIs are the application's entry point.

Every HTTP request enters the application through an endpoint.

```text
Client
    ↓
Minimal API Endpoint
    ↓
Service
    ↓
Repository
    ↓
DbContext
    ↓
Database
```

The endpoint receives the HTTP request and delegates the work to the service layer.

## Why This Matters

- Less boilerplate than controllers.
- Faster to build small APIs.
- Easier to read for simple applications.
- Supports Dependency Injection.
- Supports model binding.
- Supports validation.
- Integrates with Swagger/OpenAPI.

## Mental Model

Think of a Minimal API endpoint as the application's receptionist.

The receptionist:

- Receives the request.
- Collects the information.
- Hands the work to the correct department.
- Returns the response.

The receptionist does **not** perform the work.

Likewise, an endpoint should not contain business logic.

Its job is to receive the request and call the service layer.

## Real-World Example

Imagine calling a doctor's office.

The receptionist asks:

- Which patient?
- What appointment?
- What do you need?

The receptionist then forwards your request to the appropriate department.

The receptionist doesn't diagnose illnesses or update medical records.

A Minimal API endpoint serves the same purpose.

## Code Example

Simple endpoint:

```csharp
app.MapGet(
    "/requests/{id}",
    async (int id, IIntakeService intakeService) =>
    {
        IntakeRequest? request =
            await intakeService.FindRequestByIdAsync(id);

        return request is not null
            ? Results.Ok(request)
            : Results.NotFound();
    }
);
```

Notice what the endpoint does:

1. Receives the HTTP request.
2. Gets dependencies through Dependency Injection.
3. Calls the service.
4. Returns an HTTP response.

## Dependency Injection

Services can be injected directly into endpoint parameters.

```csharp
app.MapGet(
    "/requests",
    async (IIntakeService intakeService) =>
    {
        return Results.Ok(
            await intakeService.GetAllRequestsAsync());
    }
);
```

ASP.NET automatically provides the requested service.

No `new` keyword is required.

## Model Binding

ASP.NET automatically converts incoming data into C# objects.

For example:

```csharp
app.MapPost(
    "/requests",
    async (
        CreateRequestDto dto,
        IIntakeService intakeService) =>
    {
        ...
    }
);
```

If the request body contains:

```json
{
    "patientName": "Diane"
}
```

ASP.NET automatically creates:

```csharp
CreateRequestDto dto
```

No manual JSON parsing is required.

## Returning Responses

Minimal APIs typically return HTTP results.

Examples:

```csharp
Results.Ok(data);
```

```csharp
Results.Created("/requests/5", request);
```

```csharp
Results.BadRequest("Patient name is required.");
```

```csharp
Results.NotFound();
```

These correspond to standard HTTP status codes.

## Common Beginner Questions

### Are Minimal APIs replacing controllers?

No.

Both are fully supported in ASP.NET.

Minimal APIs are often preferred for:

- Small APIs
- Microservices
- Simple CRUD applications

Controllers are often preferred for:

- Very large applications
- Complex routing
- Large teams

### Can Minimal APIs use Dependency Injection?

Yes.

Services are injected directly into endpoint parameters.

### Can Minimal APIs use Entity Framework?

Yes.

The endpoint usually calls a service, which calls a repository, which uses Entity Framework.

### Can Minimal APIs use Swagger?

Yes.

Swagger automatically documents Minimal API endpoints.

## Common Mistakes

- Putting business logic inside endpoints.
- Accessing the database directly from endpoints.
- Forgetting to return proper HTTP status codes.
- Making endpoints responsible for validation, business logic, and data access.
- Treating `Program.cs` like one giant script instead of an API entry point.

## Interview Answer

Minimal APIs provide a lightweight way to build HTTP endpoints in ASP.NET Core without using controllers. They reduce boilerplate while supporting Dependency Injection, model binding, validation, and OpenAPI, making them well suited for small services and modern web APIs.

## One-Sentence Summary

Minimal APIs provide a simple, lightweight way to define HTTP endpoints while keeping application logic in the service layer.

## What Finally Made It Click

- A Minimal API endpoint is the application's front door.
- Endpoints receive HTTP requests, not business logic.
- The endpoint should call the service and return an HTTP response.
- Dependency Injection and model binding happen automatically.
- Small, focused endpoints are easier to read, test, and maintain.
- A boring endpoint is usually a well-designed endpoint.
````

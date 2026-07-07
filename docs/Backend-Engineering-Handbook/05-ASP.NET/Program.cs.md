# Program.cs

## What Problem Does This Solve?

Every ASP.NET Core application needs a starting point.

The application needs somewhere to:

- Create the web application
- Register services
- Configure middleware
- Define endpoints
- Start the server

In an ASP.NET Core Minimal API project, that starting point is usually `Program.cs`.

## Solution

`Program.cs` acts as the main setup file for the application.

It configures how the app is built, what services are available, how HTTP requests are handled, and which endpoints the API exposes.

In this project, `Program.cs` wires together:

- JSON options
- Swagger
- Entity Framework Core
- Dependency Injection
- Minimal API endpoints
- Development seed data
- Application startup

## Why This Matters

`Program.cs` is the composition root of the application.

That means it is the place where the major parts of the system are connected.

The individual classes do their own jobs, but `Program.cs` tells ASP.NET how those classes fit together.

Without this setup, ASP.NET would not know:

- Which service implementation to use
- Which repository implementation to use
- Which database provider to connect to
- Which routes exist
- Whether Swagger should run
- How to serialize JSON

Tiny detail, apparently applications do not configure themselves through hope and vibes. Tragic.

## Where It Fits

`Program.cs` sits at the outer edge of the application.

```text
Program.cs
    ↓
Minimal API Endpoints
    ↓
Services
    ↓
Repositories
    ↓
DbContext
    ↓
Database
```

It does not usually contain business logic.

Instead, it wires the application together.

## Mental Model

Think of `Program.cs` as the control panel.

It does not perform every task itself.

It tells the application:

- What services exist
- Where data comes from
- Which middleware should run
- Which URLs map to which actions
- How the app should behave in development

The rest of the application does the actual work.

## Main Sections

### Create the Builder

```csharp
var builder = WebApplication.CreateBuilder(args);
```

This creates the object used to configure services and application settings.

Most application setup starts with `builder`.

---

### Configure JSON

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});
```

This configures how JSON is serialized and deserialized.

In this project, it allows enum values like:

```json
"Submitted"
```

instead of numeric values like:

```json
0
```

Because making people remember that `2` means `Completed` is how software slowly becomes a cursed artifact.

---

### Add Swagger

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

Swagger generates interactive API documentation.

It allows developers to test endpoints in the browser.

---

### Register DbContext

```csharp
builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite("Data Source=clinic-intake.db")
);
```

This tells Entity Framework Core to use SQLite as the database provider.

It also registers `ClinicIntakeDbContext` with Dependency Injection.

---

### Register Services

```csharp
builder.Services.AddScoped<IIntakeRepository, EfIntakeRepository>();

builder.Services.AddScoped<IIntakeService, IntakeService>();
```

These lines tell ASP.NET:

```text
When something asks for IIntakeRepository,
provide EfIntakeRepository.

When something asks for IIntakeService,
provide IntakeService.
```

This is Dependency Injection registration.

---

### Build the App

```csharp
var app = builder.Build();
```

After services are registered, the application is built.

After this point, the app can configure middleware and endpoints.

---

### Configure Development Tools

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

Swagger is only enabled during development.

This avoids exposing development tooling unnecessarily in production.

---

### Configure Middleware

```csharp
app.UseHttpsRedirection();
```

Middleware runs during the request pipeline.

This middleware redirects HTTP requests to HTTPS.

---

### Define Endpoints

```csharp
app.MapGet(
    "/requests",
    async (
        IIntakeService intakeService,
        RequestStatus? status,
        string? patient,
        string? sort,
        int page = 1,
        int pageSize = 10
    ) =>
    {
        return Results.Ok(
            await intakeService.GetRequestSummariesAsync(
                status,
                patient,
                sort,
                page,
                pageSize));
    }
);
```

Endpoints define which HTTP routes the API supports.

This endpoint supports:

```http
GET /requests
```

with optional query parameters:

```http
GET /requests?status=Submitted&patient=di&sort=name&page=1&pageSize=10
```

---

### Seed Development Data

```csharp
using (var scope = app.Services.CreateScope())
{
    IIntakeService intakeService =
        scope.ServiceProvider.GetRequiredService<IIntakeService>();

    IEnumerable<IntakeRequest> existingRequests =
        await intakeService.GetAllRequestsAsync();

    if (!existingRequests.Any())
    {
        // Add seed data
    }
}
```

This creates sample data only when the database is empty.

The `Any()` check prevents the application from inserting duplicate seed data every time it starts.

---

### Start the Application

```csharp
app.Run();
```

This starts the web server.

Without `app.Run()`, the application configures itself beautifully and then does absolutely nothing. Very productive. Very corporate.

## Common Beginner Questions

### Is Program.cs the whole application?

No.

`Program.cs` configures the application, but most logic should live elsewhere.

Business logic belongs in services.

Data access belongs in repositories.

Database communication belongs in `DbContext`.

---

### Should business logic go in Program.cs?

Usually no.

`Program.cs` should mostly wire things together.

If endpoint logic becomes large, move it into the service layer.

---

### Why are services registered before `builder.Build()`?

Services must be registered before the application is built.

After `builder.Build()`, ASP.NET has already created the service container.

---

### Why does seed data need a scope?

`ClinicIntakeDbContext` is registered as scoped.

That means it is created within a scope.

Since seeding runs outside a normal HTTP request, the app creates a scope manually.

```csharp
using (var scope = app.Services.CreateScope())
```

---

### Why does Program.cs know about concrete classes?

`Program.cs` is allowed to know which implementation is currently being used.

That is its job as the composition root.

The service depends on `IIntakeRepository`.

`Program.cs` decides that `EfIntakeRepository` is the implementation.

## Common Mistakes

- Putting too much business logic in `Program.cs`.
- Forgetting to register services with Dependency Injection.
- Registering services with the wrong lifetime.
- Calling `await` without marking the endpoint lambda as `async`.
- Returning a `Task` instead of awaiting it.
- Forgetting to call `app.Run()`.
- Adding seed data without checking whether it already exists.

## Interview Answer

In a Minimal API project, `Program.cs` is the application startup and configuration file. It registers services with Dependency Injection, configures middleware, defines endpoints, and starts the application. It acts as the composition root where the major parts of the application are connected.

## One-Sentence Summary

`Program.cs` configures, wires together, and starts an ASP.NET Core Minimal API application.

## What Finally Made It Click

- `Program.cs` is not where all code belongs.
- It is where the application is assembled.
- Services are registered before the app is built.
- Middleware and endpoints are configured after the app is built.
- `Program.cs` is allowed to know concrete implementations because it wires the application together.
- A good `Program.cs` should configure the app, not become the entire app.

## Related Topics

### Previous

- Minimal APIs
- Dependency Injection

### Next

- Configuration
- Middleware

### See Also

- Service Layer
- Repository Pattern
- DbContext
- Swagger
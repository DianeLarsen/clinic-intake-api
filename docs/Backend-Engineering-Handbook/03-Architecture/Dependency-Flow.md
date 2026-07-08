````markdown
# Dependency Flow

## What Problem Does This Solve?

As an application grows, it becomes harder to understand how the pieces connect.

Without a clear dependency flow, code can become tangled. Endpoints may talk directly to databases, services may know too much about storage, and repositories may start handling business rules. Naturally, the software becomes a bowl of architectural spaghetti, because apparently plain noodles were not enough.

Dependency flow explains **which layer depends on which layer** and **which direction requests move through the application**.

## Solution

Keep each layer responsible for one job, and make dependencies flow in a predictable direction.

In this project, the flow is:

```text
Endpoint
    ↓
Service
    ↓
Repository
    ↓
DbContext
    ↓
Database
````

Each layer talks only to the layer directly below it.

## Where It Fits

Dependency flow is not one specific class.

It describes how the whole application is organized.

In the Clinic Intake API:

```text
Program.cs / Minimal API Endpoint
    ↓
IIntakeService
    ↓
IntakeService
    ↓
IIntakeRepository
    ↓
EfIntakeRepository
    ↓
ClinicIntakeDbContext
    ↓
SQLite Database
```

The endpoint depends on the service.

The service depends on the repository interface.

The repository depends on Entity Framework Core.

Entity Framework Core talks to the database.

## Why This Matters

* Makes the application easier to understand.
* Keeps responsibilities separated.
* Reduces tight coupling.
* Makes testing easier.
* Makes storage implementations easier to swap.
* Prevents database logic from leaking into endpoints.
* Prevents HTTP logic from leaking into services or repositories.

## Mental Model

Think of dependency flow like a chain of command.

The client makes a request.

The endpoint receives it.

The service decides what should happen.

The repository retrieves or saves data.

The database stores the data.

```text
Client Request
    ↓
Endpoint receives HTTP
    ↓
Service applies application logic
    ↓
Repository handles data access
    ↓
Database stores or returns data
```

Each layer has a role.

Each layer passes work to the next appropriate layer.

## Real-World Example

Imagine a clinic intake process.

A patient walks to the front desk.

The front desk receives the request.

The intake coordinator decides what needs to happen.

The records department retrieves or saves the chart.

The filing system stores the chart.

```text
Patient
    ↓
Front Desk
    ↓
Intake Coordinator
    ↓
Records Department
    ↓
Filing System
```

The patient does not dig through the filing cabinet.

The front desk does not rewrite storage policy.

The records department does not decide medical workflow.

Each part has a job.

## Code Example

Minimal API endpoint:

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

The endpoint calls the service:

```csharp
await intakeService.FindRequestByIdAsync(id);
```

The service calls the repository:

```csharp
public async Task<IntakeRequest?> FindRequestByIdAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}
```

The repository calls Entity Framework:

```csharp
public async Task<IntakeRequest?> GetByIdAsync(int id)
{
    return await _db.IntakeRequests
        .FirstOrDefaultAsync(r => r.Id == id);
}
```

Entity Framework queries the database.

```text
Endpoint
    ↓
Service
    ↓
Repository
    ↓
DbContext
    ↓
SQLite
```

## Dependency Inversion in the Flow

The service does not depend directly on `EfIntakeRepository`.

It depends on the interface:

```csharp
private readonly IIntakeRepository _repository;
```

That means the service only knows the repository contract.

It does not care whether the actual implementation is:

```text
InMemoryIntakeRepository
EfIntakeRepository
AzureSqlIntakeRepository
```

Dependency Injection connects the interface to the actual implementation in `Program.cs`:

```csharp
builder.Services.AddScoped<IIntakeRepository, EfIntakeRepository>();
```

## Common Beginner Questions

### Does data flow the same direction as dependencies?

Usually, the request moves downward through the layers:

```text
Endpoint
    ↓
Service
    ↓
Repository
    ↓
Database
```

Then the result comes back upward:

```text
Database
    ↑
Repository
    ↑
Service
    ↑
Endpoint
```

### Why doesn't the endpoint call the repository directly?

It could in a very small app.

But then the endpoint would start owning application logic and data access decisions.

Using a service layer keeps the endpoint focused on HTTP and keeps business logic in one place.

### Why doesn't the service use DbContext directly?

It can in smaller applications.

In this project, the repository hides the storage details so the service can stay focused on application logic.

### Why does the service depend on an interface?

Because the interface allows the repository implementation to change without changing the service.

That is Dependency Inversion.

## Common Mistakes

* Letting endpoints talk directly to the database.
* Putting HTTP response logic inside repositories.
* Putting business rules inside repositories.
* Making every layer know about every other layer.
* Skipping interfaces, then struggling to swap implementations later.
* Forgetting that dependency flow should stay predictable.

## Interview Answer

Dependency flow describes how layers in an application communicate. In a layered backend, requests usually flow from the API endpoint to the service layer, then to the repository, then to the database. Each layer has a specific responsibility, which keeps the system easier to maintain, test, and modify.

## One-Sentence Summary

Dependency flow shows how requests move through the application and keeps each layer responsible for only its part of the process.

## What Finally Made It Click

* Requests move downward through the layers.
* Results come back upward through the layers.
* Endpoints handle HTTP.
* Services handle application logic.
* Repositories handle data access.
* DbContext handles database communication.
* Interfaces keep higher layers from depending on specific implementations.
* A clean dependency flow makes the application easier to reason about.

```
```
Minimal API flow with Middleware:

HTTP Request
      │
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
DbContext
      ▼
SQLite
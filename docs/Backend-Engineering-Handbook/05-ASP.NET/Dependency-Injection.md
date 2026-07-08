# Dependency Injection in ASP.NET Core

## What Problem Does This Solve?

Applications are made of many classes that depend on each other.

For example:

```text
RequestsController
      ↓
IIntakeService
      ↓
IIntakeRepository
      ↓
ClinicIntakeDbContext
```

Without Dependency Injection, each class would create its own dependencies.

```csharp
var repository = new EfIntakeRepository();

var service = new IntakeService(repository);
```

That tightly couples classes together and makes the application harder to test, change, and maintain.

## Solution

ASP.NET Core includes a built-in Dependency Injection container.

Services are registered in `Program.cs`.

```csharp
builder.Services.AddScoped<IIntakeService, IntakeService>();
```

Then classes ask for what they need through constructors.

```csharp
public RequestsController(IIntakeService intakeService)
{
    _intakeService = intakeService;
}
```

ASP.NET creates the object and supplies the dependency automatically.

Apparently we finally taught the framework to carry its own groceries.

## Why This Matters

Dependency Injection helps with:

- Loose coupling
- Easier testing
- Cleaner architecture
- Swapping implementations
- Centralized object creation
- Managing object lifetimes

It allows classes to depend on abstractions instead of concrete implementations.

## Mental Model

Dependency Injection means:

> Classes ask for what they need instead of creating it themselves.

```text
Program.cs
    ↓
Registers services

ASP.NET DI Container
    ↓
Creates objects

Controller
    ↓
Receives service

Service
    ↓
Receives repository
```

The container is responsible for wiring the application together.

## Registering Services

Services are registered in `Program.cs`.

```csharp
builder.Services.AddScoped<IIntakeRepository, EfIntakeRepository>();

builder.Services.AddScoped<IIntakeService, IntakeService>();
```

This means:

```text
When something asks for IIntakeRepository,
give it EfIntakeRepository.

When something asks for IIntakeService,
give it IntakeService.
```

## Constructor Injection

Constructor Injection is the most common pattern in ASP.NET Core.

Example:

```csharp
public class RequestsController : ControllerBase
{
    private readonly IIntakeService _intakeService;

    public RequestsController(IIntakeService intakeService)
    {
        _intakeService = intakeService;
    }
}
```

The controller does not create the service.

It receives it.

## Dependency Flow

In the Clinic Intake API:

```text
RequestsController
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
```

Each layer depends on an abstraction where appropriate.

The controller depends on `IIntakeService`.

The service depends on `IIntakeRepository`.

The repository depends on `ClinicIntakeDbContext`.

## Service Lifetimes

ASP.NET Core services have lifetimes.

The lifetime controls how long an object exists.

### Singleton

Created once for the entire application.

```csharp
builder.Services.AddSingleton<IMyService, MyService>();
```

Use for:

- Stateless services
- Configuration-like objects
- Shared application-wide services

Be careful with shared mutable state.

Singletons live a long time. Like bad design decisions.

---

### Scoped

Created once per HTTP request.

```csharp
builder.Services.AddScoped<IIntakeService, IntakeService>();
```

Use for:

- Services that participate in a request
- Repositories
- `DbContext`

Most application services in web APIs are scoped.

---

### Transient

Created every time it is requested.

```csharp
builder.Services.AddTransient<IMyService, MyService>();
```

Use for:

- Lightweight stateless services
- Short-lived helper classes

## Why DbContext Is Scoped

Entity Framework `DbContext` is registered using:

```csharp
builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite("Data Source=clinic-intake.db"));
```

`AddDbContext()` registers the context as scoped by default.

This means one `DbContext` is created per HTTP request.

```text
HTTP Request Starts
      ↓
Create DbContext
      ↓
Controller uses Service
      ↓
Service uses Repository
      ↓
Repository uses DbContext
      ↓
SaveChangesAsync()
      ↓
Request Ends
      ↓
Dispose DbContext
```

This is important because EF Core Change Tracking depends on using the same `DbContext` during a unit of work.

## Why Interfaces Matter

Instead of depending directly on:

```csharp
EfIntakeRepository
```

the service depends on:

```csharp
IIntakeRepository
```

```csharp
public class IntakeService : IIntakeService
{
    private readonly IIntakeRepository _repository;

    public IntakeService(IIntakeRepository repository)
    {
        _repository = repository;
    }
}
```

This allows the implementation to change later.

For example:

```text
IIntakeRepository
    ├── EfIntakeRepository
    └── InMemoryIntakeRepository
```

Same service.

Different repository implementation.

## Real-World Example

The API currently registers:

```csharp
builder.Services.AddScoped<IIntakeRepository, EfIntakeRepository>();

builder.Services.AddScoped<IIntakeService, IntakeService>();
```

The controller asks for:

```csharp
IIntakeService
```

ASP.NET supplies:

```csharp
IntakeService
```

The service asks for:

```csharp
IIntakeRepository
```

ASP.NET supplies:

```csharp
EfIntakeRepository
```

The repository asks for:

```csharp
ClinicIntakeDbContext
```

ASP.NET supplies a scoped DbContext.

## Dependency Injection vs Configuration

Dependency Injection answers:

```text
What objects should be created?
```

Configuration answers:

```text
What values should the application use?
```

Example:

```csharp
builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite("Data Source=clinic-intake.db"));
```

Configuration supplies the connection string.

Dependency Injection creates the `ClinicIntakeDbContext`.

## Common Beginner Questions

### Does Dependency Injection create objects?

Yes.

The DI container creates registered services and supplies them where needed.

---

### Why not just use `new`?

Using `new` inside classes tightly couples them to specific implementations.

DI centralizes object creation and makes dependencies easier to replace.

---

### Why are services registered in `Program.cs`?

`Program.cs` is the composition root.

It is allowed to know which concrete implementations should be used.

Other classes should depend on abstractions.

---

### Why is `DbContext` scoped?

Because it should live for one request.

That allows EF Core to track changes during the request and dispose resources when the request ends.

---

### Do Controllers use Dependency Injection?

Yes.

Controllers use constructor injection.

Minimal APIs can inject dependencies directly into endpoint parameters.

## Common Mistakes

- Forgetting to register a service.
- Registering the wrong implementation.
- Using `Singleton` for services that depend on `DbContext`.
- Creating dependencies manually with `new`.
- Depending on concrete classes when an interface would be better.
- Making services static instead of letting DI manage them.
- Misunderstanding service lifetimes.

## Interview Answer

ASP.NET Core includes a built-in Dependency Injection container that manages object creation and lifetimes. Services are registered in `Program.cs`, commonly as Singleton, Scoped, or Transient. Classes receive dependencies through constructors, which reduces coupling and makes the application easier to test and maintain.

## One-Sentence Summary

Dependency Injection in ASP.NET Core centralizes object creation so classes receive the services they need instead of creating them directly.

## What Finally Made It Click

- Dependency Injection is not just about passing objects around.
- It centralizes who creates objects and how long they live.
- `Program.cs` wires the application together.
- Controllers and services ask for interfaces.
- ASP.NET supplies the concrete implementations.
- `AddScoped()` means one instance per HTTP request.
- `DbContext` is scoped because EF Core tracks changes during a request.
- Good DI design makes it possible to change the HTTP layer, repository, or database implementation without rewriting the entire application.

## Related Topics

### Previous

- Interfaces
- Dependency Injection
- Program.cs

### Next

- Middleware
- Request Lifecycle
- Testing

### See Also

- Controllers
- Service Layer
- Repository Pattern
- DbContext
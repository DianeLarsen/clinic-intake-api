# Dependency Injection (DI)

## What Problem Does This Solve?

If every class creates its own dependencies using `new`, the code becomes tightly coupled. Whenever an implementation changes (for example, switching from an in-memory repository to SQLite), multiple classes may need to be updated.

There is also no central place that shows how the application is connected together.

## Solution

Instead of creating its own dependencies, a class simply declares what it needs through its constructor.

ASP.NET creates the required objects and injects them automatically.

```csharp
public IntakeService(IIntakeRepository repository)
{
    _repository = repository;
}
```

The class doesn't care where the repository came from. It only knows it has one.

## Why This Matters

* Reduces coupling between classes.
* Centralizes object creation in one place (`Program.cs`).
* Makes implementations easy to swap.
* Simplifies testing.
* Automatically builds dependency chains.
* Makes large applications much easier to maintain.

## Mental Model

Think of ASP.NET as the **factory** or **electrician**.

Instead of every class creating and wiring its own dependencies, ASP.NET builds the objects and connects everything together.

Your classes simply declare what they need.

```text
Program.cs

↓

ASP.NET

↓

Creates Repository
Creates Service

↓

Connects Everything
```

## Real-World Example

Think about wiring a control cabinet.

The devices don't wire themselves together.

An electrician follows the wiring diagram and connects everything.

* ASP.NET is the electrician.
* `Program.cs` is the wiring diagram.
* Your classes are the devices waiting to be connected.

## Code Example

Register services in `Program.cs`:

```csharp
builder.Services.AddSingleton<
    IIntakeRepository,
    InMemoryIntakeRepository>();

builder.Services.AddSingleton<
    IIntakeService,
    IntakeService>();
```

When a class requests an `IIntakeRepository`, ASP.NET automatically creates the registered implementation and passes it into the constructor.

## Common Beginner Questions

### Isn't Dependency Injection just connecting classes together?

Yes.

More specifically, it centralizes who creates and connects those classes.

Instead of every class creating its own dependencies, ASP.NET handles all object creation and wiring.

### Why not just use `new`?

Using `new` inside every class tightly couples your code to specific implementations.

Dependency Injection makes it easy to replace implementations without changing the classes that use them.

### Does Dependency Injection create the objects?

Yes.

ASP.NET creates the objects based on the registrations in `Program.cs`.

## Common Mistakes

* Creating dependencies manually with `new` instead of requesting them through the constructor.
* Registering the wrong implementation in `Program.cs`.
* Confusing Dependency Injection with interfaces (they solve different problems).
* Using the wrong service lifetime.

## Service Lifetimes

### Singleton

* Created once for the lifetime of the application.
* Every request shares the same instance.
* Good for configuration and in-memory repositories.

### Scoped

* Created once per HTTP request.
* Each request gets its own instance.
* Used by Entity Framework Core's `DbContext`.

### Transient

* A new instance is created every time it is requested.
* Useful for lightweight, stateless services.

## Interview Answer

Dependency Injection is a design pattern where classes declare the dependencies they need instead of creating them themselves. ASP.NET manages object creation and injects those dependencies automatically, reducing coupling and making applications easier to maintain and test.

## One-Sentence Summary

Dependency Injection centralizes the creation and wiring of objects so classes can focus on their own responsibilities instead of creating their own dependencies.

## What Finally Made It Click

* It centralizes who creates and wires objects.
* ASP.NET becomes the factory (or electrician).
* Classes don't create dependencies; they simply request what they need.

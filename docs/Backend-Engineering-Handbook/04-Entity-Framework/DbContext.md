# DbContext

## What Problem Does This Solve?

The application needs a way to communicate with the database. Without a central database object, each part of the application would need to know how to connect, query, track changes, and save data.

That would spread database logic throughout the application, making the code harder to maintain.

## Solution

A `DbContext` is Entity Framework Core's main object for working with the database.

It represents a session with the database and knows how C# models map to database tables.

## Why This Matters

* Provides one central gateway to the database.
* Tracks changes to objects.
* Knows which tables exist.
* Converts LINQ queries into SQL.
* Saves inserts, updates, and deletes.
* Works with Dependency Injection.

## Mental Model

Think of `DbContext` as the application's **database control panel**.

The application does not talk directly to the database everywhere.

Instead, it goes through the `DbContext`.

```text
Application

↓

DbContext

↓

Database
```

## Real-World Example

In the Clinic Intake API, `ClinicIntakeDbContext` represents the clinic intake database.

It knows there is a table of intake requests and gives the application a way to query and save them.

## Code Example

```csharp
using ClinicIntakeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicIntakeApi.Data;

public class ClinicIntakeDbContext : DbContext
{
    public ClinicIntakeDbContext(
        DbContextOptions<ClinicIntakeDbContext> options)
        : base(options)
    {
    }

    public DbSet<IntakeRequest> IntakeRequests => Set<IntakeRequest>();
}
```

Registering it in `Program.cs`:

```csharp
builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite("Data Source=clinic-intake.db"));
```

## Common Beginner Questions

### Is DbContext the database?

No.

The `DbContext` is not the database itself. It is the C# object EF Core uses to communicate with the database.

### Is DbContext like PrismaClient?

Yes.

`DbContext` fills a similar role in EF Core that `PrismaClient` fills in Prisma.

It is the main object used to query and save data.

### Why does DbContext use Dependency Injection?

ASP.NET creates a `DbContext` for each request and injects it where needed.

This keeps database work organized and prevents different requests from accidentally sharing the same database session.

### Why is DbContext usually scoped?

A `DbContext` is normally created once per HTTP request.

This is called a **scoped** lifetime.

Each request gets its own database session.

## Common Mistakes

* Treating `DbContext` as a long-lived singleton.
* Forgetting to register it in `Program.cs`.
* Trying to manually create it everywhere with `new`.
* Forgetting to call `SaveChanges()` or `SaveChangesAsync()`.

## Interview Answer

A `DbContext` is the main EF Core object that represents a session with the database. It tracks changes, exposes tables through `DbSet` properties, converts LINQ queries to SQL, and saves changes back to the database.

## One-Sentence Summary

A `DbContext` is EF Core's main gateway between the C# application and the database.

## What Finally Made It Click

* `DbContext` is like `PrismaClient` for EF Core.
* It is not the database itself.
* It is the object the application uses to talk to the database.

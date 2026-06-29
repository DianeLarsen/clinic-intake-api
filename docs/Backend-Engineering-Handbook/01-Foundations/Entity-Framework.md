# Entity Framework Core (EF Core)

## What Problem Does This Solve?

The application currently stores data in memory using a `List<IntakeRequest>`. This data disappears whenever the application restarts.

Applications need persistent storage so data can survive restarts and eventually be stored in a real database such as SQLite or Azure SQL.

## Solution

Entity Framework Core (EF Core) is an **Object Relational Mapper (ORM)** that connects C# objects to database tables.

Instead of writing SQL for every database operation, developers work with C# classes while EF Core handles most of the database interaction.

## Why This Matters

* Stores data permanently.
* Lets developers work with C# objects instead of raw SQL.
* Automatically maps objects to database tables.
* Tracks changes to objects.
* Supports database migrations.
* Makes moving from SQLite to Azure SQL much easier.

## Mental Model

Think of EF Core as a **translator**.

```text
C# Objects

↓

Entity Framework Core

↓

SQL

↓

Database
```

You work in C#.

EF Core translates your code into SQL.

## Real-World Example

Instead of writing SQL like:

```sql
SELECT * FROM IntakeRequests
WHERE Status = 'Completed';
```

you write:

```csharp
_db.IntakeRequests
    .Where(r => r.Status == RequestStatus.Completed);
```

EF Core translates the LINQ query into SQL and executes it for you.

## Code Example

```csharp
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

Example usage:

```csharp
_db.IntakeRequests.Add(request);

_db.SaveChanges();
```

## Common Beginner Questions

### What is an ORM?

ORM stands for **Object Relational Mapper**.

It maps C# objects to database tables and database rows back into C# objects.

### What is a DbContext?

A `DbContext` is EF Core's connection to the database.

It knows:

* What tables exist.
* How models map to tables.
* What objects have changed.
* How to save those changes.

Think of it as the application's **gateway to the database**.

### What is a DbSet?

A `DbSet<T>` represents a table in the database.

```csharp
public DbSet<IntakeRequest> IntakeRequests => Set<IntakeRequest>();
```

A `DbSet` allows you to query, add, update, and delete records for that model.

### Why use EF Core instead of writing SQL?

EF Core lets developers stay in C# while automatically generating SQL behind the scenes.

It also provides:

* Automatic object mapping.
* Change tracking.
* Database migrations.
* Easier database portability.

### Is EF Core like Prisma or Drizzle?

Yes.

EF Core fills a similar role in C# that Prisma and Drizzle do in the JavaScript ecosystem.

The concepts are nearly identical even though the syntax is different.

## Common Mistakes

* Forgetting to call `SaveChanges()`.
* Thinking a `DbSet` is just a list instead of a database table.
* Writing raw SQL when EF Core can already perform the task.
* Forgetting that EF Core tracks changes to loaded objects.

## Architecture

Before EF Core:

```text
Endpoint

↓

Service

↓

Repository

↓

List<IntakeRequest>
```

With EF Core:

```text
Endpoint

↓

Service

↓

Repository

↓

DbContext

↓

SQLite Database
```

Later:

```text
Endpoint

↓

Service

↓

Repository

↓

DbContext

↓

Azure SQL
```

## Interview Answer

Entity Framework Core is Microsoft's ORM for .NET. It allows developers to work with C# objects while automatically mapping those objects to database tables, generating SQL, tracking changes, and managing database interactions.

## One-Sentence Summary

Entity Framework Core is an ORM that lets C# objects work directly with database tables through `DbContext` and `DbSet`, reducing the need to write repetitive SQL.

## What Finally Made It Click

* EF Core is basically Prisma or Drizzle for C#.
* `DbContext` is the gateway to the database.
* `DbSet` represents a database table.
* EF Core translates C# into SQL behind the scenes.

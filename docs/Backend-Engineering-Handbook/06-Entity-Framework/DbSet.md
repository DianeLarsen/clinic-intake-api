# DbSet

## What Problem Does This Solve?

The application needs a way to represent database tables in C# code.

Without `DbSet`, the application would not have a clean way to query, add, update, or delete records for a specific model type.

## Solution

A `DbSet<T>` represents a database table for a specific C# model.

For example, a `DbSet<IntakeRequest>` represents the `IntakeRequests` table.

## Why This Matters

* Represents a table in C#.
* Allows querying records with LINQ.
* Allows adding new records.
* Allows updating and deleting records.
* Works with EF Core change tracking.
* Keeps database access strongly typed.

## Mental Model

Think of a `DbSet` as a **table-shaped collection**.

It looks like a collection in C#, but it represents a real database table.

```text
DbSet<IntakeRequest>

↓

IntakeRequests table
```

It is not just a normal list. EF Core can turn operations on a `DbSet` into SQL.

## Real-World Example

In the Clinic Intake API:

```csharp
public DbSet<IntakeRequest> IntakeRequests => Set<IntakeRequest>();
```

This tells EF Core that the application has a table of intake requests.

## Code Example

Query all requests:

```csharp
_db.IntakeRequests.ToList();
```

Find one request:

```csharp
_db.IntakeRequests.FirstOrDefault(r => r.Id == id);
```

Add a request:

```csharp
_db.IntakeRequests.Add(request);
_db.SaveChanges();
```

Filter completed requests:

```csharp
_db.IntakeRequests
    .Where(r => r.Status == RequestStatus.Completed);
```

## Common Beginner Questions

### Is DbSet the same as a List?

No.

A `List<T>` is an in-memory collection.

A `DbSet<T>` represents a database table.

When you query a `DbSet`, EF Core can translate that query into SQL.

### Does DbSet immediately run a database query?

Not always.

Some LINQ queries are not executed until you enumerate them or call methods like:

```csharp
ToList()
FirstOrDefault()
Count()
```

This is called deferred execution.

### Can I add items directly to a DbSet?

Yes.

```csharp
_db.IntakeRequests.Add(request);
```

But the change is not saved to the database until:

```csharp
_db.SaveChanges();
```

or later:

```csharp
await _db.SaveChangesAsync();
```

## Common Mistakes

* Thinking `DbSet` is just a list.
* Forgetting that queries may not run immediately.
* Adding records but forgetting to call `SaveChanges()`.
* Returning database entities directly from every API endpoint without considering DTOs.

## Interview Answer

A `DbSet<T>` represents a table for a specific entity type in EF Core. It allows the application to query, add, update, and delete records using C# and LINQ while EF Core translates those operations into database commands.

## One-Sentence Summary

A `DbSet<T>` is EF Core's C# representation of a database table.

## What Finally Made It Click

* `DbSet` is like a table exposed in C#.
* It looks collection-like, but it is not just a `List`.
* EF Core translates operations on a `DbSet` into SQL.
* DbContext is the database.
* DbSet is one table inside that database.
* You almost never think about a DbSet without its DbContext.


### Does a DbSet always belong to a DbContext?

Yes.

A `DbSet` always belongs to a `DbContext`.

The `DbContext` represents the entire database, while each `DbSet` represents one table within that database.

Think of it like this:
ClinicIntakeDbContext
│
├── IntakeRequests
├── Patients
├── Doctors
└── Appointments


The `DbContext` provides the connection to the database, and the `DbSet` provides access to a specific table.
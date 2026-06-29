# LINQ

## What Problem Does This Solve?

Applications constantly need to work with collections of data.

For example:

* Find one request by ID.
* Find all completed requests.
* Count how many requests exist.
* Sort requests alphabetically.
* Search for requests by patient name.

Without LINQ, these operations require writing loops and conditional statements repeatedly.

## Solution

LINQ (**Language Integrated Query**) provides a standard way to query, filter, sort, and transform collections using C#.

Instead of writing loops, developers write queries.

For example:

```csharp
requests.Where(r => r.Status == RequestStatus.Completed);
```

This reads almost like English:

> Return the requests **where** the status is Completed.

## Why This Matters

* Makes code easier to read.
* Reduces repetitive loops.
* Works with in-memory collections (`List<T>`).
* Works with databases through Entity Framework Core.
* Allows complex queries to be built step by step.

## Mental Model

Think of LINQ as a **filter pipeline**.

You start with a collection:

```text
All Requests
```

Each LINQ method narrows or transforms the results.

```text
All Requests
      │
      ▼
Where(Status == Completed)
      │
      ▼
OrderBy(PatientName)
      │
      ▼
Count()
```

Each step builds on the previous one.

## Works With More Than Lists

One of LINQ's biggest strengths is that the syntax stays the same regardless of where the data comes from.

In-memory collection:

```csharp
requests.Where(r => r.Status == RequestStatus.Completed);
```

Database table:

```csharp
_db.IntakeRequests
    .Where(r => r.Status == RequestStatus.Completed);
```

When using Entity Framework Core, LINQ is translated into SQL automatically.

The same C# query can search either memory or a database.

## LINQ Methods Learned

### Where()

Filters a collection.

Example:

```csharp
requests.Where(r => r.Status == RequestStatus.Completed);
```

Returns only the items that satisfy the condition.

---

### FirstOrDefault()

Returns the first matching item or `null` if none exists.

Example:

```csharp
requests.FirstOrDefault(r => r.Id == id);
```

Commonly used when searching by a unique value.

---

### Count()

Returns the number of items.

Example:

```csharp
requests.Count();
```

Can also count matching items.

```csharp
requests.Count(r => r.Status == RequestStatus.Completed);
```

## Building Queries

One of LINQ's greatest strengths is that queries can be built incrementally.

Example:

```csharp
IEnumerable<IntakeRequest> requests = _repository.GetAll();

if (status is not null)
{
    requests = requests.Where(r => r.Status == status.Value);
}

return requests;
```

Instead of creating separate methods for every filter combination, the query is built based on the user's input.

## Common Beginner Questions

### Does Where() change the original collection?

No.

`Where()` returns a new filtered sequence.

The original collection is unchanged.

### Why assign the result back to the variable?

```csharp
requests = requests.Where(...);
```

Because `Where()` returns a new sequence.

If you ignore the return value, nothing changes.

### Does LINQ always use SQL?

No.

LINQ works with many different data sources.

With a `List<T>`, LINQ searches memory.

With Entity Framework Core, LINQ is translated into SQL and executed by the database.

## Common Mistakes

* Forgetting to use the value returned by `Where()`.
* Thinking LINQ modifies the original collection.
* Writing loops when a LINQ method already exists.
* Assuming LINQ only works with databases.

## Interview Answer

LINQ (Language Integrated Query) provides a consistent way to query, filter, sort, and transform collections in C#. The same syntax works with in-memory collections and with databases through Entity Framework Core.

## One-Sentence Summary

LINQ provides a readable, consistent way to query and manipulate collections using C# instead of manual loops.

## What Finally Made It Click

* LINQ is not just for databases.
* The same syntax works with both `List<T>` and `DbSet<T>`.
* Each LINQ method returns a new sequence, allowing queries to be built one step at a time.
* Entity Framework Core translates LINQ into SQL automatically when querying a database.
* LINQ is C#'s standard query language for working with collections of data. Entity Framework is one technology that understands LINQ and translates it into SQL.
* LINQ is C#'s built-in way of querying collections. Entity Framework is the tool that understands LINQ and translates it into SQL when the collection happens to be a database table.
| JavaScript / TypeScript | C#                              |
| ----------------------- | ------------------------------- |
| Classes                 | Classes                         |
| Interfaces              | Interfaces                      |
| `array.filter()`        | `Where()` (LINQ)                |
| `array.map()`           | `Select()` (LINQ)               |
| `array.sort()`          | `OrderBy()` (LINQ)              |
| Drizzle / Prisma        | Entity Framework Core           |
| PostgreSQL              | SQLite / SQL Server / Azure SQL |

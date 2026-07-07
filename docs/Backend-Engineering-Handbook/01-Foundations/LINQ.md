# LINQ

## What Problem Does This Solve?

Applications constantly need to work with collections of data.

For example:

- Find one request by ID.
- Find all completed requests.
- Search for patients by name.
- Sort requests alphabetically.
- Count matching requests.
- Convert entities into DTOs.

Without LINQ, these operations require writing loops and conditional statements repeatedly.

Example:

```csharp
List<IntakeRequest> completed = [];

foreach (IntakeRequest request in requests)
{
    if (request.Status == RequestStatus.Completed)
    {
        completed.Add(request);
    }
}
```

This works, but it becomes repetitive and difficult to read as queries become more complex.

## Solution

LINQ (**Language Integrated Query**) provides a standard way to query, filter, sort, transform, and aggregate collections using C#.

Instead of writing loops, developers build queries.

Example:

```csharp
requests.Where(r =>
    r.Status == RequestStatus.Completed);
```

This reads almost like English:

> Return the requests **where** the status is Completed.

## Why This Matters

LINQ makes code:

- More readable
- Less repetitive
- Easier to maintain
- Consistent across different data sources

The same LINQ syntax works with:

- Arrays
- Lists
- `IEnumerable<T>`
- `DbSet<T>` (Entity Framework Core)

This consistency is one of LINQ's greatest strengths.

## Mental Model

Think of LINQ as a pipeline.

You start with a collection.

```text
All Requests
```

Each LINQ method performs one operation.

```text
All Requests
      │
      ▼
Where()
      │
      ▼
OrderBy()
      │
      ▼
Skip()
      │
      ▼
Take()
      │
      ▼
Select()
      │
      ▼
Results
```

Each method builds on the previous one.

## LINQ Works Everywhere

One of LINQ's biggest strengths is that the syntax stays the same regardless of where the data comes from.

### In Memory

```csharp
List<IntakeRequest> requests = [];

requests.Where(r =>
    r.Status == RequestStatus.Completed);
```

### Database

```csharp
_db.IntakeRequests
    .Where(r =>
        r.Status == RequestStatus.Completed);
```

The code looks almost identical.

The difference is what happens behind the scenes.

With a `List<T>`, LINQ searches objects already in memory.

With Entity Framework Core, LINQ is translated into SQL and executed by the database.

## Building Queries

One of LINQ's greatest strengths is that queries can be built incrementally.

```csharp
IEnumerable<IntakeRequest> requests =
    await _repository.GetAllAsync();

if (status is not null)
{
    requests = requests.Where(r =>
        r.Status == status.Value);
}

if (!string.IsNullOrWhiteSpace(patient))
{
    requests = requests.Where(r =>
        r.PatientName.Contains(
            patient,
            StringComparison.OrdinalIgnoreCase));
}

if (sort == "name")
{
    requests = requests.OrderBy(r =>
        r.PatientName);
}
```

Rather than creating a separate method for every possible combination of filters, one query is built step by step.

## LINQ Methods You've Used

### Filtering

```csharp
Where()
```

Keeps only matching items.

---

### Searching

```csharp
Contains()
```

Finds partial text matches.

---

### Sorting

```csharp
OrderBy()

OrderByDescending()
```

Changes the order of the results.

---

### Pagination

```csharp
Skip()

Take()
```

Returns only one page of data.

---

### Projection

```csharp
Select()
```

Creates new objects from existing ones.

Example:

```csharp
.Select(r =>
    new RequestSummaryDto
    {
        Id = r.Id,
        DisplayText =
            $"{r.PatientName} - {r.Status}"
    });
```

---

### Aggregation

```csharp
Count()

Any()
```

Returns information about the collection rather than the collection itself.

## Real-World Example

The Clinic Intake API builds queries like this:

```text
All Requests
      │
      ▼
Filter by Status
      │
      ▼
Search by Patient Name
      │
      ▼
Sort Alphabetically
      │
      ▼
Count Results
      │
      ▼
Skip()
      │
      ▼
Take()
      │
      ▼
Select DTO
      │
      ▼
Return API Response
```

Every step is another LINQ operation.

Each operation builds on the previous one.

## Common Beginner Questions

### Does LINQ modify the original collection?

No.

Most LINQ methods return a new sequence.

```csharp
requests = requests.Where(...);
```

not

```csharp
requests.Where(...);
```

If you ignore the return value, nothing changes.

---

### Does LINQ always use SQL?

No.

LINQ is part of C#.

It works with many different data sources.

Entity Framework Core is one technology that understands LINQ and translates it into SQL.

---

### Is LINQ slower than writing loops?

Usually not enough to matter.

LINQ prioritizes readability and maintainability.

When used with Entity Framework Core, LINQ allows the database to perform the work efficiently.

## Common Mistakes

- Forgetting to assign the returned sequence.
- Thinking LINQ modifies the original collection.
- Believing LINQ only works with databases.
- Writing manual loops when a LINQ method already exists.
- Forgetting that the order of LINQ operations matters.

## Interview Answer

LINQ (Language Integrated Query) is C#'s built-in query language for working with collections. It provides a consistent syntax for filtering, searching, sorting, transforming, and aggregating data. The same LINQ syntax works with in-memory collections and with databases through Entity Framework Core.

## One-Sentence Summary

LINQ provides a readable, consistent way to query and manipulate collections without writing manual loops.

## What Finally Made It Click

- LINQ isn't just for databases.
- LINQ isn't Entity Framework.
- LINQ is C#'s standard query language for collections.
- Entity Framework Core understands LINQ and translates it into SQL.
- Most LINQ methods return a new sequence, allowing queries to be built one step at a time.
- LINQ is essentially a pipeline where each operation performs one small transformation before passing the results to the next step.
- Once I realized that nearly every backend API follows the pattern **Filter → Search → Sort → Count → Paginate → Select**, LINQ stopped feeling like a collection of unrelated methods and started feeling like a language for building data pipelines.

## Related Topics

### Previous

- Collections
- Lambdas

### Next

- LINQ and EF Core
- Async/Await

### See Also

- Filtering
- Searching
- Sorting
- Pagination
- DTO Mapping
- Entity Framework
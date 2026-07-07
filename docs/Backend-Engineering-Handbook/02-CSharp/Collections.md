# Collections

## What Problem Does This Solve?

Applications rarely work with a single object.

Instead, they usually manage groups of related objects.

For example:

- A list of patient requests.
- A collection of appointments.
- A list of clinics.
- A set of users.

Without collections, you would need a separate variable for every object.

```csharp
IntakeRequest request1;
IntakeRequest request2;
IntakeRequest request3;
```

This quickly becomes impossible to maintain.

## Solution

Collections allow a program to store and work with multiple objects as a single unit.

C# provides several collection types, each designed for different situations.

Some of the most common are:

- Arrays
- List<T>
- IEnumerable<T>
- DbSet<T>

## Why This Matters

Collections are one of the most fundamental building blocks in C#.

Almost every backend application works with collections because databases rarely return only one record.

Collections also make it possible to:

- Search data
- Filter data
- Sort data
- Count items
- Transform objects
- Iterate through results

Most LINQ methods operate on collections.

## Mental Model

Think of a collection as a container.

Instead of carrying one object, it carries many.

```text
┌──────────────────────┐
│ IntakeRequest        │
└──────────────────────┘

Single object
```

vs.

```text
┌────────────────────────────┐
│ IntakeRequest              │
│ IntakeRequest              │
│ IntakeRequest              │
│ IntakeRequest              │
└────────────────────────────┘

Collection
```

Most backend code works with collections rather than individual objects.

## The Collections You've Used

### Arrays

Arrays have a fixed size.

```csharp
string[] names =
[
    "Alice",
    "Bob",
    "Charlie"
];
```

Arrays are useful when the number of items never changes.

---

### List<T>

A `List<T>` is a resizable collection.

```csharp
List<string> names = [];

names.Add("Alice");

names.Add("Bob");
```

Lists can grow and shrink during execution.

Most in-memory collections start as a `List<T>`.

---

### IEnumerable<T>

`IEnumerable<T>` represents a sequence of items.

```csharp
IEnumerable<IntakeRequest> requests;
```

Unlike `List<T>`, `IEnumerable<T>` focuses on reading data rather than modifying it.

Many LINQ methods return `IEnumerable<T>`.

For example:

```csharp
requests = requests.Where(r =>
    r.Status == RequestStatus.Completed);
```

The result is another sequence of requests.

---

### DbSet<T>

Entity Framework uses `DbSet<T>`.

```csharp
DbSet<IntakeRequest>
```

A `DbSet<T>` represents an entire database table.

Although it behaves like a collection, the data is stored in a database rather than memory.

LINQ works with `DbSet<T>` the same way it works with `List<T>`.

## Choosing the Right Collection

### Use an Array

When:

- The size never changes.

Example:

```csharp
string[] weekdays =
[
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday"
];
```

---

### Use List<T>

When:

- Items will be added or removed.

Example:

```csharp
List<IntakeRequest> requests = [];
```

---

### Use IEnumerable<T>

When:

- You only need to read or query the data.

Example:

```csharp
IEnumerable<IntakeRequest> requests =
    _repository.GetAll();
```

This communicates:

> "I'm working with a sequence of items."

rather than

> "I'm managing a mutable list."

## Collections and LINQ

LINQ operates on collections.

For example:

```csharp
requests
    .Where(r => r.Status == RequestStatus.Completed)
    .OrderBy(r => r.PatientName)
    .Take(10);
```

Each LINQ method returns another collection.

The original collection is unchanged.

## Real-World Example

Your project retrieves every intake request.

```csharp
IEnumerable<IntakeRequest> requests =
    await _repository.GetAllAsync();
```

The service then:

- Filters

```csharp
Where()
```

- Searches

```csharp
Contains()
```

- Sorts

```csharp
OrderBy()
```

- Paginates

```csharp
Skip()

Take()
```

Every one of these operations works because the data is stored in a collection.

## Common Beginner Questions

### Why not always use List<T>?

Because many methods only need to read data.

Returning `IEnumerable<T>` communicates that callers should treat the results as a sequence rather than modifying them.

---

### Is DbSet<T> a List?

Not exactly.

It behaves similarly, but represents a database table.

LINQ queries against a `DbSet<T>` are translated into SQL by Entity Framework Core.

---

### Why does LINQ usually return IEnumerable<T>?

Because LINQ produces sequences of results.

It doesn't necessarily create a new list immediately.

This allows queries to be chained together efficiently.

## Common Mistakes

- Using an array when the size needs to change.
- Returning `List<T>` when only enumeration is needed.
- Thinking `DbSet<T>` stores data in memory.
- Confusing a single object with a collection of objects.
- Assuming every collection behaves exactly the same.

## Interview Answer

Collections allow applications to store and work with multiple objects. C# provides several collection types such as arrays, `List<T>`, `IEnumerable<T>`, and `DbSet<T>`, each designed for different scenarios. LINQ operates on these collections to filter, sort, search, and transform data.

## One-Sentence Summary

Collections provide a structured way to store and work with multiple objects as a single unit.

## What Finally Made It Click

- A collection is simply a container for multiple objects.
- Different collections solve different problems.
- Arrays have a fixed size.
- Lists can grow and shrink.
- `IEnumerable<T>` represents a sequence of items to read or query.
- `DbSet<T>` represents a database table.
- LINQ works on collections, whether the data comes from memory or a database.
- Most backend programming is really about retrieving, transforming, and returning collections of data.

## Related Topics

### Previous

- Classes
- Generics

### Next

- Lambdas
- LINQ

### See Also

- Entity Framework
- LINQ and EF Core
- Pagination
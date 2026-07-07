# Lambdas

## What Problem Does This Solve?

Programs often need to perform small operations on data.

For example:

- Find completed requests.
- Sort by patient name.
- Convert an `IntakeRequest` into a `RequestSummaryDto`.
- Count matching items.

Before lambda expressions, these operations required creating separate methods.

Example:

```csharp
public bool IsCompleted(IntakeRequest request)
{
    return request.Status == RequestStatus.Completed;
}

requests.Where(IsCompleted);
```

For small pieces of logic, creating an entire method is unnecessary.

## Solution

A lambda expression is a short, anonymous function.

Instead of writing a separate method, the logic can be written inline.

Example:

```csharp
requests.Where(r =>
    r.Status == RequestStatus.Completed);
```

This reads as:

> For each request (`r`), return whether its status is Completed.

## Why This Matters

Lambda expressions make code:

- Shorter
- Easier to read
- Easier to maintain
- Ideal for LINQ queries

Modern C# relies heavily on lambdas.

You'll encounter them throughout:

- LINQ
- Entity Framework Core
- ASP.NET Core
- Dependency Injection
- Events
- Delegates

## Mental Model

Think of a lambda as a tiny function that only exists where it's needed.

Instead of writing:

```text
Function somewhere else

↓

Call function
```

you write:

```text
Tiny function

↓

Use immediately
```

The function has no name because it never needs one.

## Lambda Syntax

General form:

```csharp
parameter => expression
```

The arrow (`=>`) can be read as:

> "goes to"

or

> "maps to"

Example:

```csharp
r => r.PatientName
```

Read as:

> Take a request and return its patient name.

## Real-World Examples

### Filtering

```csharp
requests.Where(r =>
    r.Status == RequestStatus.Completed);
```

For each request...

Return whether it is completed.

---

### Searching

```csharp
requests.Where(r =>
    r.PatientName.Contains(patient));
```

For each request...

Check whether the patient's name contains the search text.

---

### Sorting

```csharp
requests.OrderBy(r =>
    r.PatientName);
```

For each request...

Return the value that should be used for sorting.

---

### Selecting

```csharp
requests.Select(r =>
    new RequestSummaryDto
    {
        Id = r.Id,
        DisplayText = $"{r.PatientName} - {r.Status}"
    });
```

For each request...

Create a new DTO.

## Breaking Down a Lambda

Example:

```csharp
r => r.PatientName
```

Breaks down into:

```text
r

↓

Current object

↓

PatientName

↓

Return this value
```

It is equivalent to:

```csharp
public string GetPatientName(
    IntakeRequest r)
{
    return r.PatientName;
}
```

The lambda simply removes the need to write an entire method.

## Expression vs Block Lambdas

### Expression Lambda

Returns one value.

```csharp
r => r.PatientName
```

---

### Block Lambda

Contains multiple statements.

```csharp
r =>
{
    Console.WriteLine(r.PatientName);

    return r.PatientName;
}
```

Expression lambdas are much more common in LINQ.

## Code Example

Filtering completed requests:

```csharp
requests.Where(r =>
    r.Status == RequestStatus.Completed);
```

Sorting alphabetically:

```csharp
requests.OrderBy(r =>
    r.PatientName);
```

Creating DTOs:

```csharp
requests.Select(r =>
    new RequestSummaryDto
    {
        Id = r.Id,
        DisplayText =
            $"{r.PatientName} - {r.Status}"
    });
```

## Common Beginner Questions

### What does `r` mean?

Nothing special.

It is simply the parameter name.

This works exactly the same:

```csharp
request =>
    request.PatientName
```

Most developers use short names because lambdas are usually very small.

### Is a lambda a loop?

No.

Methods like `Where()` or `Select()` loop through the collection.

The lambda only describes what should happen for each item.

### Can lambdas return objects?

Yes.

For example:

```csharp
.Select(r =>
    new RequestSummaryDto
    {
        Id = r.Id,
        DisplayText =
            $"{r.PatientName} - {r.Status}"
    });
```

Each object becomes a new DTO.

## Common Mistakes

- Thinking `r` is a keyword.
- Believing the lambda performs the loop.
- Writing large amounts of logic inside a lambda.
- Forgetting that the arrow represents a function.

## Interview Answer

A lambda expression is a concise, anonymous function used to pass behavior as data. Lambdas are commonly used with LINQ to filter, sort, search, and transform collections without creating separate methods.

## One-Sentence Summary

A lambda is a small anonymous function that describes what should happen to each item in a collection.

## What Finally Made It Click

- A lambda is just a tiny function.
- The parameter (`r`) represents the current item.
- The arrow (`=>`) separates the input from the result.
- LINQ performs the looping; the lambda only describes the operation.
- Most LINQ methods take a lambda because they need instructions for each item in the collection.
- Once I realized `r => r.PatientName` is simply a shorthand version of a one-line method, lambdas stopped feeling like special syntax and started feeling like ordinary functions.

## Related Topics

### Previous

- Collections
- Generics

### Next

- LINQ

### See Also

- Filtering
- Searching
- Sorting
- DTO Mapping
- LINQ and EF Core
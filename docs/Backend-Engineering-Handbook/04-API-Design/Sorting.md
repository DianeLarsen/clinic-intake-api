````markdown
# Sorting

## What Problem Does This Solve?

Data often needs to be presented in a meaningful order.

For example, a user may want to see:

- Patients alphabetically.
- Most recent requests first.
- Oldest requests first.
- Completed requests grouped together.

Without sorting, records are usually returned in whatever order the database happens to store them, which is often unpredictable and not useful.

## Solution

Sorting allows clients to choose the order in which records are returned.

Instead of creating multiple endpoints, the client specifies the desired sort order using a query parameter.

Example:

```http
GET /requests?sort=name
```

The API returns the requests sorted alphabetically by patient name.

## Where It Fits

Sorting is part of the API design.

The request flows through the application like this:

```text
Client
    ↓
Endpoint
    ↓
Service
    ↓
Repository
    ↓
Database
```

The endpoint receives the sort option.

The service applies the appropriate ordering.

The repository retrieves the sorted data.

## Why This Matters

- Makes data easier to read.
- Gives clients control over how results are displayed.
- Reduces work on the client.
- Avoids creating multiple endpoints for different sort orders.
- Works naturally with filtering, searching, and pagination.

## Mental Model

Think of sorting like organizing papers on your desk.

The information is the same.

Only the order changes.

You might organize by:

- Name
- Date
- Priority

Sorting changes **how** records are presented, not **which** records are returned.

## Real-World Example

Imagine a receptionist looking for a patient.

A list sorted like this:

```text
Tyler
Alice
Emma
Ryan
Charlie
```

is much harder to scan than:

```text
Alice
Charlie
Emma
Ryan
Tyler
```

Sorting helps users find information more quickly.

## Code Example

The endpoint accepts an optional sort parameter:

```csharp
app.MapGet(
    "/api/v1/requests",
    async (
        IIntakeService intakeService,
        string? sort
    ) =>
    {
        ...
    }
);
```

The service applies the appropriate ordering.

Ascending:

```csharp
if (sort == "name")
{
    requests = requests.OrderBy(r =>
        r.PatientName);
}
```

Descending:

```csharp
if (sort == "name_desc")
{
    requests = requests.OrderByDescending(r =>
        r.PatientName);
}
```

The rest of the query continues using the newly ordered collection.

## Ascending vs Descending

Ascending:

```http
GET /requests?sort=name
```

Produces:

```text
Alice
Bob
Charlie
Diane
```

Descending:

```http
GET /requests?sort=name_desc
```

Produces:

```text
Diane
Charlie
Bob
Alice
```

## Sorting vs Filtering

Sorting changes the **order** of records.

Filtering changes **which** records are returned.

Example:

```http
GET /requests?status=Completed&sort=name
```

Step 1:

Filter:

```text
Completed Requests
```

Step 2:

Sort:

```text
Alice
Charlie
Ryan
```

The set of records stays the same.

Only the order changes.

## Combining Queries

Sorting works together with filtering and searching.

Example:

```http
GET /requests?status=Submitted&patient=an&sort=name
```

The query is built step by step:

```csharp
requests = requests.Where(...);

requests = requests.Where(...);

requests = requests.OrderBy(...);
```

Each LINQ method builds on the previous one.

## Common Beginner Questions

### Why not sort on the client?

The server usually has access to the complete dataset.

Sorting on the server means:

- Less data transferred.
- Better performance.
- Consistent ordering for all clients.

### Why use `OrderBy()` instead of `Sort()`?

`OrderBy()` is a LINQ method that returns a new sequence.

It works with both in-memory collections and Entity Framework queries.

### Can multiple fields be sorted?

Yes.

LINQ provides:

```csharp
OrderBy()

ThenBy()

OrderByDescending()

ThenByDescending()
```

These allow sorting by multiple properties.

## Common Mistakes

- Sorting after pagination.
- Creating separate endpoints for every sort option.
- Confusing sorting with filtering.
- Forgetting that `OrderBy()` returns a new sequence.
- Hardcoding one sort order when clients may need flexibility.

## Interview Answer

Sorting allows clients to control the order of returned data using query parameters. LINQ methods such as `OrderBy()` and `OrderByDescending()` provide a readable way to order collections while keeping the API flexible and efficient.

## One-Sentence Summary

Sorting changes the order of returned records without changing which records are included.

## What Finally Made It Click

- Sorting answers the question, **"In what order should the records appear?"**
- `OrderBy()` sorts ascending.
- `OrderByDescending()` sorts descending.
- Sorting does not remove or add records.
- Sorting should happen **before pagination** so each page contains the correct records.
- Sorting works naturally with filtering and searching as part of one LINQ query.
````

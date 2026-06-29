````markdown
# Filtering

## What Problem Does This Solve?

Applications often store much more data than a client needs.

For example, if there are 10,000 intake requests, a nurse may only want to see requests that are:

- Submitted
- In Review
- Completed

Without filtering, every request would return every record, forcing the client to search through the results itself.

## Solution

Filtering allows clients to request only the records that match specific criteria.

Instead of returning every intake request, the API returns only those that satisfy the filter.

Example:

```http
GET /requests?status=Completed
```

The API returns only completed requests.

## Where It Fits

Filtering is part of the API design.

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

The endpoint receives the filter values.

The service builds the query.

The repository retrieves the matching data.

## Why This Matters

- Reduces unnecessary data transfer.
- Improves performance.
- Makes APIs easier to use.
- Allows one endpoint to serve many different requests.
- Prevents creating dozens of specialized endpoints.

## Mental Model

Think of filtering like searching for books in a library.

Without filtering:

```text
Give me every book.
```

With filtering:

```text
Give me only mystery books.
```

The library doesn't hand you every book and expect you to sort through them yourself.

The API shouldn't either.

## Real-World Example

Imagine a clinic dashboard.

A nurse only wants to see patients currently waiting.

Instead of downloading every intake request, the dashboard requests:

```http
GET /requests?status=Submitted
```

The server returns only submitted requests.

## Code Example

The endpoint receives the filter value:

```csharp
app.MapGet(
    "/requests",
    async (
        IIntakeService intakeService,
        RequestStatus? status
    ) =>
    {
        return Results.Ok(
            await intakeService.GetRequestSummariesAsync(
                status,
                null,
                null,
                1,
                10));
    }
);
```

The service applies the filter:

```csharp
if (status is not null)
{
    requests = requests.Where(r =>
        r.Status == status.Value);
}
```

Only matching requests continue through the query.

## Multiple Filters

Filters can be combined.

Example:

```http
GET /requests?status=Completed&patient=dia
```

The query becomes:

```csharp
requests = requests
    .Where(r => r.Status == status.Value)
    .Where(r =>
        r.PatientName.Contains(
            patient,
            StringComparison.OrdinalIgnoreCase));
```

Each filter narrows the results further.

## Common Beginner Questions

### Why not create separate endpoints?

Instead of creating:

```http
GET /completedRequests

GET /submittedRequests

GET /inReviewRequests
```

one endpoint can support all of them:

```http
GET /requests?status=Completed
```

This keeps the API smaller and easier to maintain.

### Where should filtering happen?

Filtering belongs in the service layer.

The endpoint receives the filter.

The service builds the query.

The repository retrieves the data.

### Can filters be optional?

Yes.

If a filter is not supplied, it is simply skipped.

Example:

```csharp
if (status is not null)
{
    requests = requests.Where(...);
}
```

## Common Mistakes

- Creating a new endpoint for every filter.
- Filtering after data has already been returned to the client.
- Putting filtering logic inside the endpoint.
- Forgetting that filters should usually be optional.
- Mixing filtering with sorting or pagination before the query is fully built.

## Interview Answer

Filtering allows clients to request only the data they need by supplying query parameters. Instead of creating many specialized endpoints, one endpoint can support multiple filtering options while the service layer builds the appropriate query.

## One-Sentence Summary

Filtering narrows a collection so the client receives only the records that match specific criteria.

## What Finally Made It Click

- Filtering answers the question, **"Which records do I want?"**
- One flexible endpoint is better than many specialized endpoints.
- Filters are usually optional.
- The service layer builds the query one filter at a time.
- LINQ's `Where()` is the primary tool for filtering collections.
- Filtering should happen before sorting, pagination, and DTO mapping.
```
````

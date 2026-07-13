````markdown
# Pagination

## What Problem Does This Solve?

Applications often contain far more data than should be returned in a single request.

Imagine an application with:

- 100 requests
- 10,000 requests
- 1 million requests

Returning every record would:

- Slow down the response.
- Waste bandwidth.
- Use unnecessary memory.
- Make the client process far more data than needed.

## Solution

Pagination divides a large collection into smaller pages.

Instead of returning every record, the client requests only the page it needs.

Example:

```http
GET /requests?page=2&pageSize=10
```

The API returns only records 11–20.

## Where It Fits

Pagination is part of API design.

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

The endpoint receives the page information.

The service builds the query.

The repository retrieves only the requested records.

## Why This Matters

- Faster responses.
- Less memory usage.
- Smaller network transfers.
- Better user experience.
- Scales to very large datasets.
- Standard practice for production APIs.

## Mental Model

Think of a large book.

Nobody expects to receive the entire book every time they want to read it.

Instead they request:

```text
Page 1

Page 2

Page 3
```

Pagination works the same way.

The API returns only the "page" of records the client requested.

## Real-World Example

Imagine a hospital dashboard with 25,000 patient records.

The nurse only needs to see the first 20.

The dashboard requests:

```http
GET /requests?page=1&pageSize=20
```

When the nurse clicks "Next":

```http
GET /requests?page=2&pageSize=20
```

The server sends only those records.

## Code Example

The endpoint accepts pagination parameters:

```csharp
app.MapGet(
    "/api/v1/requests",
    async (
        IIntakeService intakeService,
        int page = 1,
        int pageSize = 10
    ) =>
    {
        ...
    }
);
```

The service applies pagination using LINQ:

```csharp
requests = requests
    .Skip((page - 1) * pageSize)
    .Take(pageSize);
```

For example:

```text
Page = 1
Page Size = 10
```

```text
Skip 0

Take 10
```

Returns:

```text
Records 1-10
```

---

```text
Page = 2
Page Size = 10
```

```text
Skip 10

Take 10
```

Returns:

```text
Records 11-20
```

---

```text
Page = 3
Page Size = 10
```

```text
Skip 20

Take 10
```

Returns:

```text
Records 21-30
```

## Why Skip Comes Before Take

Imagine a deck of cards.

You don't take the first ten cards and then skip ten.

Instead you:

1. Skip the cards from previous pages.
2. Take the next group.

```text
All Records
      │
      ▼
Skip()
      │
      ▼
Take()
```

This is why the order matters.

## Pagination Metadata

Many production APIs return more than just the items.

Example:

```json
{
    "page": 2,
    "pageSize": 10,
    "totalCount": 57,
    "totalPages": 6,
    "items": [
        ...
    ]
}
```

This allows clients to know:

- How many pages exist.
- Whether a "Next" button should be enabled.
- Whether the current page is the last page.

At the moment, this project returns only the requested items. Adding pagination metadata is a common improvement for production APIs.

## Order of Operations

Pagination should happen after filtering, searching, and sorting.

```text
All Requests
      │
      ▼
Filter
      │
      ▼
Search
      │
      ▼
Sort
      │
      ▼
Skip
      │
      ▼
Take
      │
      ▼
Return Results
```

If pagination happens first, each page could contain the wrong records.

## Common Beginner Questions

### Why not return everything?

Returning all records becomes increasingly expensive as the dataset grows.

Pagination keeps responses fast and manageable.

### Why use both page and pageSize?

`page` determines which group of records to return.

`pageSize` determines how many records each page contains.

Example:

```http
GET /requests?page=3&pageSize=25
```

Returns records 51–75.

### Why use Skip() and Take()?

`Skip()` ignores records from previous pages.

`Take()` returns only the requested number of records.

Together they efficiently retrieve one page of data.

## Common Mistakes

- Returning every record.
- Applying pagination before filtering or sorting.
- Forgetting that page numbers usually start at 1.
- Allowing an unlimited page size.
- Confusing page number with record number.

## Interview Answer

Pagination allows APIs to return large datasets in smaller, manageable pages using query parameters such as `page` and `pageSize`. LINQ's `Skip()` and `Take()` methods efficiently retrieve only the requested records, improving performance, scalability, and user experience.

## One-Sentence Summary

Pagination divides large datasets into smaller pages so clients receive only the records they need.

## What Finally Made It Click

- Pagination answers the question, **"Which slice of the data do I need?"**
- `Skip()` moves past records from previous pages.
- `Take()` returns only the requested number of records.
- Pagination should happen after filtering, searching, and sorting.
- Most production APIs return pagination metadata such as `totalCount` and `totalPages`.
- Pagination improves scalability by reducing the amount of data transferred in each request.
````

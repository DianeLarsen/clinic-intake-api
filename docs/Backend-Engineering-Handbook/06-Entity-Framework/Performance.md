# Performance

## What Problem Does This Solve?

Applications often work with thousands or millions of database records.

Poor queries can:

- Use excessive memory
- Perform unnecessary database calls
- Return far more data than needed
- Slow down APIs

Entity Framework Core provides several techniques to keep queries efficient.

---

# Why This Matters

Database performance affects:

- API response time
- Server memory usage
- CPU usage
- Database load
- User experience

Small improvements in a query can have a significant impact as an application grows.

---

# Mental Model

Think of a database like a warehouse.

If you ask:

> "Bring me everything."

Workers search the whole warehouse.

If you ask:

> "Bring me aisle 4, shelf 2."

The job finishes much faster.

Good queries ask for only what they need.

---

# 1. Filter Early

Good:

```csharp
var requests = await _db.IntakeRequests
    .Where(r => r.Status == RequestStatus.Completed)
    .ToListAsync();
```

Bad:

```csharp
var requests = await _db.IntakeRequests.ToListAsync();

requests = requests
    .Where(r => r.Status == RequestStatus.Completed)
    .ToList();
```

The first version lets SQL perform the filtering.

The second loads every record into memory first.

---

# 2. Select Only What You Need

Returning an entire entity is often unnecessary.

Instead of:

```csharp
var requests = await _db.IntakeRequests
    .ToListAsync();
```

Project directly into a DTO.

```csharp
var summaries = await _db.IntakeRequests
    .Select(r => new RequestSummaryDto
    {
        Id = r.Id,
        PatientName = r.PatientName
    })
    .ToListAsync();
```

Benefits:

- Less memory
- Smaller SQL queries
- Faster API responses

---

# 3. Use Pagination

Avoid:

```csharp
.ToListAsync()
```

when thousands of records exist.

Instead:

```csharp
.Skip((page - 1) * pageSize)
.Take(pageSize)
```

This loads only one page.

---

# 4. Use AsNoTracking() for Read-Only Queries

Normally EF tracks every entity it loads.

Tracking consumes memory.

For read-only queries:

```csharp
var requests = await _db.IntakeRequests
    .AsNoTracking()
    .ToListAsync();
```

Benefits:

- Lower memory usage
- Faster queries
- Less work for Change Tracker

Only use this when you do not plan to modify the entities.

---

# 5. Avoid N+1 Queries

Bad:

```text
Load 100 requests

↓

Load one clinic

↓

Repeat 100 times
```

101 SQL queries.

Better:

```csharp
.Include(r => r.Clinic)
```

One SQL query.

---

# 6. Don't Overuse Include()

This is also bad:

```csharp
.Include(r => r.Patient)
.Include(r => r.Clinic)
.Include(r => r.Doctor)
.Include(r => r.Appointments)
.Include(r => r.Medications)
```

If the API only needs:

- Patient name
- Clinic name

loading everything wastes resources.

Only include data the response needs.

---

# 7. Let SQL Do the Work

Operations like:

- Where()
- OrderBy()
- Skip()
- Take()
- Select()

should happen before calling:

```csharp
.ToListAsync()
```

Once ToListAsync() executes, the query has already run.

Everything afterward happens in memory.

---

# Query Pipeline

Good query order:

```text
Database

↓

Where()

↓

OrderBy()

↓

Skip()

↓

Take()

↓

Select()

↓

ToListAsync()
```

Notice that ToListAsync() is last.

---

# Common Mistakes

Loading every record

```csharp
.ToListAsync()
```

before filtering.

---

Returning entire entities instead of DTOs.

---

Using Include() for data the response never uses.

---

Creating N+1 queries.

---

Tracking objects that will never be modified.

---

# Performance Checklist

Before writing a query ask:

✓ Can I filter earlier?

✓ Can SQL do this instead of C#?

✓ Do I really need every column?

✓ Should I project to a DTO?

✓ Am I loading too many related entities?

✓ Should I use AsNoTracking()?

✓ Should this endpoint support pagination?

---

# Interview Answer

Entity Framework performance comes from loading only the data required for the current operation. Common optimizations include filtering in SQL, projecting directly into DTOs, using pagination, avoiding N+1 queries with Include(), using AsNoTracking() for read-only queries, and loading only the relationships needed by the application.

---

# One-Sentence Summary

Fast EF Core queries load the smallest amount of data needed while allowing SQL Server (or SQLite) to do as much work as possible.

---

# What Finally Made It Click

- Databases are optimized to search data.
- SQL should filter before C# receives the results.
- DTOs improve performance, not just API design.
- Include() is helpful, but only when related data is actually needed.
- Pagination prevents loading unnecessary rows.
- Change Tracking has a cost.
- The fastest query is the one that retrieves only what the current screen or API response actually needs.

---

# Related Topics

### Previous

- Relationships
- Lazy vs Eager Loading
- LINQ and EF Core
- Change Tracking

### Next

- ASP.NET
- Testing

### See Also

- LINQ
- DTO Mapping
- Pagination
- API Responses
# Lazy vs Eager Loading

## What Problem Does This Solve?

When entities have relationships, Entity Framework Core needs to know when related data should be loaded.

For example, an intake request may be related to:

* Patient
* Clinic
* Provider
* Appointments
* Notes

Loading all related data every time would waste memory and slow down queries.

Loading too little data can cause missing information or extra database queries.

## Solution

Entity Framework Core supports different loading strategies.

The main ones are:

* Eager Loading
* Lazy Loading
* Explicit Loading

Each strategy answers the same question:

> When should related data be loaded?

## Why This Matters

Loading related data affects:

* Query performance
* Memory usage
* Number of database calls
* API response speed
* Risk of N+1 query problems

Backend developers should load only the data needed for the current use case.

## Mental Model

Think of loading related data like packing for a trip.

Eager loading:

```text
I know I need this, so pack it now.
```

Lazy loading:

```text
Do not pack it unless I ask for it later.
```

Explicit loading:

```text
Wait for me to decide, then load it manually.
```

## Eager Loading

Eager loading loads related data immediately.

In EF Core, this is done with `Include()`.

```csharp
var requests = await _db.IntakeRequests
    .Include(r => r.Patient)
    .Include(r => r.Clinic)
    .ToListAsync();
```

This tells EF:

> Load intake requests, patients, and clinics now.

EF usually generates a SQL JOIN.

## When to Use Eager Loading

Use eager loading when the API response needs the related data.

Example:

```json
{
  "patientName": "Alice",
  "clinicName": "Monroe Clinic"
}
```

Since the response needs the clinic name, the query should include the clinic.

## Lazy Loading

Lazy loading waits until the related property is accessed.

Conceptually:

```csharp
var request = await _db.IntakeRequests
    .FirstOrDefaultAsync(r => r.Id == id);

Console.WriteLine(request.Clinic.Name);
```

With lazy loading enabled, EF would query the clinic when `request.Clinic` is accessed.

EF Core does not enable lazy loading by default.

## Why Lazy Loading Can Be Dangerous

Lazy loading can cause the N+1 query problem.

Example:

```text
1 query to load 100 requests
+
100 queries to load each clinic
=
101 queries
```

This can severely slow down an API.

Convenient code can create terrible database behavior. Humanity continues its proud tradition of making easy things expensive.

## Explicit Loading

Explicit loading loads related data later, but only when the developer asks.

Example:

```csharp
var request = await _db.IntakeRequests
    .FirstOrDefaultAsync(r => r.Id == id);

await _db.Entry(request)
    .Reference(r => r.Clinic)
    .LoadAsync();
```

This gives the developer full control over when related data is loaded.

## Include and ThenInclude

`Include()` loads a directly related entity.

```csharp
.Include(r => r.Patient)
```

`ThenInclude()` loads related data from a related entity.

Example:

```csharp
var clinics = await _db.Clinics
    .Include(c => c.Patients)
        .ThenInclude(p => p.IntakeRequests)
    .ToListAsync();
```

This loads:

```text
Clinic
  ↓
Patients
  ↓
IntakeRequests
```

This connected structure is called an object graph.

## The N+1 Query Problem

The N+1 problem happens when an application loads one set of records, then performs one additional query for each record.

Bad pattern:

```text
Load requests
    ↓
For each request, load clinic
```

If there are 100 requests, this can become 101 database queries.

Better pattern:

```csharp
var requests = await _db.IntakeRequests
    .Include(r => r.Clinic)
    .ToListAsync();
```

This loads the needed data in one query.

## Choosing a Loading Strategy

### Use Eager Loading

When you know the response needs related data.

```csharp
.Include(r => r.Clinic)
```

### Use Explicit Loading

When you might need related data later, but only under certain conditions.

```csharp
await _db.Entry(request)
    .Reference(r => r.Clinic)
    .LoadAsync();
```

### Be Careful With Lazy Loading

Lazy loading is convenient but can hide database queries.

Many teams avoid it because it makes performance harder to predict.

## Common Beginner Questions

### Why is my navigation property null?

Because EF did not load it.

The foreign key may exist, but the related object may not be in memory.

### Does `Include()` always make things faster?

No.

It helps when you need related data.

It hurts when it loads data the response does not use.

### Should I always use `Include()`?

No.

Only include related data required for the current use case.

### What is an object graph?

An object graph is a connected set of related objects.

Example:

```text
Clinic
  ↓
Patients
  ↓
IntakeRequests
```

## Common Mistakes

* Assuming related objects load automatically.
* Using too many `Include()` calls.
* Returning huge object graphs from APIs.
* Accidentally creating N+1 queries.
* Loading full entities when a DTO projection would be enough.

## Interview Answer

Entity Framework Core supports different strategies for loading related data. Eager loading uses `Include()` to load related data immediately, lazy loading loads related data only when accessed, and explicit loading loads related data manually when requested. Choosing the right strategy helps avoid unnecessary data loading and N+1 query problems.

## One-Sentence Summary

Lazy, eager, and explicit loading are different strategies for deciding when related data should be loaded from the database.

## What Finally Made It Click

* Relationships tell EF what data is connected.
* Loading strategy tells EF when to retrieve that connected data.
* `Include()` means load related data now.
* Lazy loading means load it when accessed.
* Explicit loading means load it later only when I ask.
* Navigation properties can be null if related data was not loaded.
* The best query depends on what the API response or screen actually needs.
* Just because EF can load an entire object graph does not mean it should. Databases are powerful, not psychic, and they will obediently fetch a mountain of data if asked.

## Related Topics

### Previous

* Relationships
* Change Tracking

### Next

* Performance
* DTO Mapping

### See Also

* LINQ and EF Core
* API Responses
* Pagination
* Repository Pattern

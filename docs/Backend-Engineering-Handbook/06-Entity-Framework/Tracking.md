# Change Tracking

## What Problem Does This Solve?

When an application changes an object, the database does not automatically know about those changes.

For example:

```csharp
request.UpdateStatus(RequestStatus.Completed);
```

This only changes the C# object in memory.

Without a way to detect changes, developers would have to manually tell Entity Framework every property that changed before updating the database.

## Solution

Entity Framework Core automatically tracks entities that it loads from the database.

When an entity is retrieved using a `DbContext`, EF Core stores both:

- The original values
- The current values

When `SaveChanges()` or `SaveChangesAsync()` is called, EF Core compares the two versions and generates the necessary SQL.

## Why This Matters

Change Tracking allows developers to work with normal C# objects instead of writing SQL.

Benefits include:

- Automatic UPDATE statements
- Automatic INSERT statements
- Automatic DELETE statements
- Only changed values are written to the database
- Multiple changes can be saved with a single database call

## Mental Model

Think of EF Core as taking a snapshot of every object it loads.

```text
Database
    │
    ▼
Load Entity
    │
    ▼
Original Snapshot
    │
    ▼
Developer modifies object
    │
    ▼
Current Values
    │
    ▼
SaveChangesAsync()
    │
    ▼
Compare Original vs Current
    │
    ▼
Generate SQL
```

EF Core isn't watching the database.

It's watching your C# objects.

## The Four Entity States

Every tracked entity exists in one of four primary states.

### Unchanged

The object matches the database.

```text
Database
Status = Submitted

Object
Status = Submitted
```

No SQL is generated.

---

### Added

A new object has been added.

```csharp
_db.IntakeRequests.Add(request);
```

When saved:

```sql
INSERT INTO IntakeRequests (...)
```

---

### Modified

An existing object has changed.

```csharp
request.UpdateStatus(RequestStatus.Completed);
```

When saved:

```sql
UPDATE IntakeRequests
SET Status = 'Completed'
```

---

### Deleted

An object has been marked for removal.

```csharp
_db.IntakeRequests.Remove(request);
```

When saved:

```sql
DELETE FROM IntakeRequests
WHERE Id = ...
```

## SaveChangesAsync()

Nothing is written to the database until:

```csharp
await _db.SaveChangesAsync();
```

This method:

1. Looks at every tracked entity.
2. Determines its current state.
3. Generates SQL.
4. Executes the SQL inside a transaction.
5. Updates the tracked state.

Think of it as synchronizing memory with the database.

## Tracked vs Detached Entities

### Tracked Entity

Loaded from Entity Framework.

```csharp
var request =
    await _db.IntakeRequests
        .FirstOrDefaultAsync(r => r.Id == id);
```

EF Core automatically tracks it.

You can modify it directly.

```csharp
request.UpdateStatus(status);

await _db.SaveChangesAsync();
```

No call to `Update()` is required.

---

### Detached Entity

Created somewhere else.

For example:

- JSON from an API request
- Another application
- A manually created object

```csharp
var request = new IntakeRequest();
```

EF Core has never seen this object.

It is **Detached**.

To begin tracking it:

```csharp
_db.Update(request);
```

or

```csharp
_db.Attach(request);
```

depending on the scenario.

## Why Most APIs Query First

Instead of:

```csharp
_db.Update(requestFromJson);
```

Most APIs do:

```csharp
var request =
    await _db.IntakeRequests
        .FirstOrDefaultAsync(r => r.Id == id);

request.UpdateStatus(dto.Status);

await _db.SaveChangesAsync();
```

Why?

Because the database contains the complete entity.

The DTO usually contains only a few fields.

Loading the existing entity prevents accidentally overwriting data that the client never intended to change.

## Multiple Changes

EF Core does not send SQL every time a property changes.

Example:

```csharp
request.UpdateStatus(RequestStatus.Completed);

request.PatientName = "Diane";

request.UpdateStatus(RequestStatus.InReview);

await _db.SaveChangesAsync();
```

Only one SQL statement is generated.

EF Core only compares:

- Original values
- Final values

Intermediate values are ignored because they never reached the database.

## AsNoTracking()

Sometimes data is only being displayed.

Example:

```csharp
await _db.IntakeRequests
    .AsNoTracking()
    .ToListAsync();
```

This tells EF Core:

> "Return the objects, but don't track them."

Benefits:

- Faster queries
- Less memory usage
- No change detection overhead

Use `AsNoTracking()` for read-only data.

## Common Beginner Questions

### Does changing an object update the database?

No.

Only the object changes.

The database changes after calling:

```csharp
await _db.SaveChangesAsync();
```

---

### Why don't I call Update() after changing an entity?

Because Entity Framework is already tracking entities that it loaded.

It already knows they changed.

---

### When should I use Update()?

When the entity did **not** come from Entity Framework.

For example:

- JSON request
- External service
- Detached entity

---

### Why use AsNoTracking()?

Tracking consumes memory.

If the entity will never be modified, tracking is unnecessary.

## Common Mistakes

- Forgetting to call `SaveChangesAsync()`.
- Calling `Update()` on an entity that is already being tracked.
- Using `Update()` with partial DTOs and accidentally overwriting data.
- Assuming property changes immediately update the database.
- Tracking thousands of read-only entities instead of using `AsNoTracking()`.

## Interview Answer

Entity Framework Core uses Change Tracking to monitor entities loaded through a `DbContext`. It remembers their original values, detects changes made in memory, and generates the appropriate SQL when `SaveChanges()` is called. This allows developers to work with C# objects instead of manually writing UPDATE, INSERT, and DELETE statements.

## One-Sentence Summary

Change Tracking allows Entity Framework Core to automatically detect changes to entities and synchronize those changes with the database when `SaveChanges()` is called.

## What Finally Made It Click

- EF Core is not watching the database; it is watching the C# objects it loaded.
- `SaveChangesAsync()` is a synchronization step, not simply an update method.
- Entity states (`Added`, `Modified`, `Deleted`, `Unchanged`) determine which SQL statements EF Core generates.
- Multiple object changes become a single SQL statement because EF compares the original object with its final state.
- Tracked entities don't require `Update()`. Detached entities do.
- `AsNoTracking()` skips change tracking entirely, making read-only queries faster.
- A good API usually loads the existing entity first, updates only the intended properties, and then calls `SaveChangesAsync()`. This avoids accidentally overwriting data that wasn't part of the request.

## Related Topics

### Previous

- DbContext
- DbSet
- Migrations

### Next

- Relationships
- Lazy vs Eager Loading
- Performance

### See Also

- Repository Pattern
- Service Layer
- Dependency Injection
- LINQ and EF Core
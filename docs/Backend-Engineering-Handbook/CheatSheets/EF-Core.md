# EF Core Cheat Sheet

## Install

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite

dotnet add package Microsoft.EntityFrameworkCore.Design

dotnet add package Microsoft.EntityFrameworkCore.Tools
```

---

## Register DbContext

```csharp
builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite(
        "Data Source=clinic-intake.db"));
```

---

## DbContext

```csharp
public class ClinicIntakeDbContext : DbContext
{
    public ClinicIntakeDbContext(
        DbContextOptions<ClinicIntakeDbContext> options)
        : base(options)
    {
    }

    public DbSet<IntakeRequest> IntakeRequests =>
        Set<IntakeRequest>();
}
```

---

## Add

```csharp
_db.IntakeRequests.Add(request);

await _db.SaveChangesAsync();
```

---

## Find One

```csharp
await _db.IntakeRequests
    .FirstOrDefaultAsync(r => r.Id == id);
```

---

## Get All

```csharp
await _db.IntakeRequests.ToListAsync();
```

---

## Delete

```csharp
_db.IntakeRequests.Remove(request);

await _db.SaveChangesAsync();
```

---

## Save Changes

```csharp
await _db.SaveChangesAsync();
```

---

## Common Async Methods

```csharp
ToListAsync()

FirstOrDefaultAsync()

AnyAsync()

CountAsync()

SaveChangesAsync()
```

---

## LINQ

```csharp
Where()

Select()

OrderBy()

OrderByDescending()

Skip()

Take()

Count()

Any()
```

---

## Migrations

Create

```bash
dotnet ef migrations add InitialCreate
```

Update Database

```bash
dotnet ef database update
```

Remove Last Migration

```bash
dotnet ef migrations remove
```

---

## Mental Model

```text
DbContext

↓

DbSet<T>

↓

LINQ

↓

SQL

↓

Database
```

---

## Things to Remember

- DbContext represents a database session.
- DbSet<T> represents a table.
- LINQ queries become SQL.
- SaveChangesAsync() writes changes to the database.
- Use async EF methods whenever possible.
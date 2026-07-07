# Relationships

## What Problem Does This Solve?

Real applications rarely store everything in one table.

For example, an intake request belongs to a patient, and a patient belongs to a clinic.

If all clinic and patient information were stored directly inside every intake request, the same data would be repeated many times.

That causes problems when data changes.

## Solution

Relationships connect tables using foreign keys.

Instead of duplicating clinic information in every intake request, the request stores a `ClinicId`.

```text
Clinic
  1
  ↓
many
IntakeRequests
```

The database stores IDs.

C# uses navigation properties to work with related objects.

## Why This Matters

Relationships:

* Reduce duplicate data.
* Keep data consistent.
* Model real-world connections.
* Allow queries across related tables.
* Make databases easier to maintain.

## Mental Model

Think of a foreign key as a pointer.

```text
IntakeRequest.ClinicId
        ↓
Clinic.Id
```

The intake request does not store the entire clinic.

It stores the ID of the clinic it belongs to.

## Relationship Types

### One-to-Many

One record is related to many records.

Example:

```text
One Clinic
    ↓
Many Patients
```

```text
One Patient
    ↓
Many IntakeRequests
```

### Many-to-Many

Many records relate to many other records.

Example:

```text
Clinics
    ↓
ClinicSpecialties
    ↓
Specialties
```

A clinic can have many specialties.

A specialty can belong to many clinics.

## Code Example

Clinic:

```csharp
public class Clinic
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public List<Patient> Patients { get; set; } = [];

    public List<IntakeRequest> Requests { get; set; } = [];
}
```

Patient:

```csharp
public class Patient
{
    public int Id { get; set; }

    public string FullName { get; set; } = "";

    public int ClinicId { get; set; }

    public Clinic? Clinic { get; set; }

    public List<IntakeRequest> IntakeRequests { get; set; } = [];
}
```

IntakeRequest:

```csharp
public class IntakeRequest
{
    public int Id { get; set; }

    public string PatientName { get; set; } = "";

    public RequestStatus Status { get; private set; }

    public int ClinicId { get; set; }

    public Clinic? Clinic { get; set; }

    public int PatientId { get; set; }

    public Patient? Patient { get; set; }
}
```

## Foreign Keys vs Navigation Properties

Foreign keys are stored in the database.

```csharp
public int ClinicId { get; set; }
```

Navigation properties are used by C#.

```csharp
public Clinic? Clinic { get; set; }
```

The foreign key tells the database which row is related.

The navigation property lets the developer access the related object.

## Why Navigation Properties Are Nullable

Even if the database requires a relationship, EF Core may not have loaded the related object.

Example:

```csharp
var request = await _db.IntakeRequests
    .FirstOrDefaultAsync(r => r.Id == id);
```

This loads the request, but not necessarily the clinic.

```csharp
request.Clinic
```

may still be `null`.

The relationship exists in the database, but the related object may not be loaded into memory.

## Migration Strategy

When adding a required relationship to an existing table, use a safe migration path.

```text
1. Add the new table.
2. Add the foreign key as nullable.
3. Backfill existing rows.
4. Change the foreign key to required.
5. Add/enforce the constraint.
```

This avoids breaking existing data.

## Common Beginner Questions

### Can a clinic exist without patients?

Yes.

A parent record can exist without child records.

### Can a patient exist without a clinic?

In this project, no.

The business rule says every patient belongs to a clinic.

### Why not store clinic name directly on IntakeRequest?

Because that duplicates data.

If the clinic name changes, every intake request would need to be updated.

### Why keep both `ClinicId` and `Clinic`?

`ClinicId` is for the database.

`Clinic` is for C# code.

## Common Mistakes

* Duplicating related data instead of using relationships.
* Making a required foreign key before backfilling existing rows.
* Assuming navigation properties are always loaded.
* Confusing foreign keys with navigation properties.
* Deleting and recreating a database instead of learning migration/backfill patterns.

## Interview Answer

Relationships connect tables using keys. In Entity Framework Core, foreign key properties represent the database relationship, while navigation properties allow developers to work with related objects in C#. A one-to-many relationship is common when one parent record, such as a clinic, has many child records, such as patients or intake requests.

## One-Sentence Summary

Relationships connect database tables using foreign keys while navigation properties make those connections easier to work with in C#.

## What Finally Made It Click

* The database stores IDs.
* C# works with objects.
* Foreign keys connect rows.
* Navigation properties connect objects.
* A parent can exist without children.
* A required child must point to a valid parent.
* When adding required relationships to existing data, add nullable first, backfill, then make required.
* Relationships are how databases stop repeating the same information over and over like a badly designed spreadsheet.

## Related Topics

### Previous

* DbContext
* DbSet
* Migrations
* Change Tracking

### Next

* Lazy vs Eager Loading
* Performance

### See Also

* Repository Pattern
* Service Layer
* DTO Mapping
* API Responses

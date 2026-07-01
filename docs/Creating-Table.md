Good. This is the right way to learn migrations, because “drop DB and recreate” is fine for toy data, but not how grown-up systems behave. Allegedly we are becoming grown-ups. Horrifying.

We’ll add **Patient** as a new parent/related entity, then connect `IntakeRequest` to `Patient`.

## Target relationship

```text
Clinic
  1
  ↓
many
Patients

Patient
  1
  ↓
many
IntakeRequests
```

So:

```text
Clinic can exist without patients.
Patient must belong to a clinic.
IntakeRequest should belong to a patient.
```

## Step 1: Create `Models/Patient.cs`

```csharp
namespace ClinicIntakeApi.Models;

public class Patient
{
    public int Id { get; set; }

    public string FullName { get; set; } = "";

    // Required relationship:
    // every patient belongs to one clinic.
    public int ClinicId { get; set; }

    // Navigation property:
    // lets C# access patient.Clinic.
    public Clinic? Clinic { get; set; }

    // One patient can have many intake requests.
    public List<IntakeRequest> IntakeRequests { get; set; } = [];
}
```

## Step 2: Update `IntakeRequest`

Add:

```csharp
public int? PatientId { get; set; }

public Patient? Patient { get; set; }
```

Use `int?` for now because existing intake requests already exist. This lets old rows survive until we backfill. Databases prefer valid history over our sudden enlightenment.

So your model will roughly include:

```csharp
public int ClinicId { get; set; }

public Clinic? Clinic { get; set; }

public int? PatientId { get; set; }

public Patient? Patient { get; set; }
```

## Step 3: Update `Clinic`

Add patients:

```csharp
public List<Patient> Patients { get; set; } = [];
```

So:

```csharp
namespace ClinicIntakeApi.Models;

public class Clinic
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public List<IntakeRequest> Requests { get; set; } = [];

    public List<Patient> Patients { get; set; } = [];
}
```

## Step 4: Update `ClinicIntakeDbContext`

Add:

```csharp
public DbSet<Patient> Patients => Set<Patient>();
```

Full-ish version:

```csharp
public DbSet<IntakeRequest> IntakeRequests => Set<IntakeRequest>();

public DbSet<Clinic> Clinics => Set<Clinic>();

public DbSet<Patient> Patients => Set<Patient>();
```

## Step 5: Build first

```bash
dotnet build
```

Fix compile errors before migrations. Never migrate broken code. That is how databases learn to hate you.

## Step 6: Create migration

```bash
dotnet ef migrations add AddPatients
```

This should create:

```text
Patients table
PatientId column on IntakeRequests
Foreign key PatientId → Patients.Id
Foreign key ClinicId → Clinics.Id
```

## Step 7: Update database

```bash
dotnet ef database update
```

## Step 8: Seed patients after clinics

In seed data, after clinics are saved, add patients:

```csharp
Patient[] patients =
[
    new Patient { FullName = "Alice", ClinicId = clinics[0].Id },
    new Patient { FullName = "Bob", ClinicId = clinics[1].Id },
    new Patient { FullName = "Charlie", ClinicId = clinics[2].Id },
];

db.Patients.AddRange(patients);

await db.SaveChangesAsync();
```

Then when creating intake requests:

```csharp
Patient patient = patients[i % patients.Length];

IntakeRequest request =
    await intakeService.AddRequestAsync(
        patient.FullName,
        patient.ClinicId);

request.PatientId = patient.Id;
```

But since `request` is returned tracked from EF, this needs a save after setting `PatientId`:

```csharp
await db.SaveChangesAsync();
```

That works, though later we’ll make it cleaner by updating `AddRequestAsync()` to accept `patientId`.

## Important

For now:

```csharp
public int? PatientId { get; set; }
```

Later, after every request has a patient, we can make it required:

```csharp
public int PatientId { get; set; }
```

That is the mature migration path:

```text
add nullable column
backfill data
make required
```

Tiny steps. Fewer database tantrums.

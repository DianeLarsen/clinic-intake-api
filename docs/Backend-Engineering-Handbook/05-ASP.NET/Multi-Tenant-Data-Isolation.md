# Multi-Tenant Data Isolation

## Problem

The Clinic Intake API stores data for several clinics in the same database.

Each clinic is a tenant:

```text
Clinic 1 -> Patients and requests owned by Clinic 1
Clinic 2 -> Patients and requests owned by Clinic 2
Clinic 3 -> Patients and requests owned by Clinic 3
```

A valid token must not grant access to every tenant. Without explicit filtering, an authenticated Clinic 1 user could potentially retrieve, update, or delete Clinic 2 records by guessing their IDs.

## Security Boundary

The trusted clinic identity comes from the validated JWT:

```text
JWT ClinicId claim
    -> Controller
    -> Service
    -> Repository
    -> Database query
```

The client does not choose its clinic through JSON, a route value, or a query parameter. Those inputs are controlled by the caller and cannot define the security boundary.

## Layer Responsibilities

### Controller

The controller reads the validated claim:

```csharp
int clinicId = User.GetRequiredClinicId();
```

It passes that value to every service operation:

```csharp
await _intakeService.FindRequestByIdAsync(
    id,
    clinicId
);
```

The controller does not query the database or accept `ClinicId` from the request body.

### Service

The service requires `clinicId` in its method contracts:

```csharp
Task<IntakeRequest?> FindRequestByIdAsync(
    int id,
    int clinicId
);
```

Making the parameter mandatory prevents callers from accidentally using an unscoped version of the operation.

When creating a request, the service searches for the patient inside the authenticated clinic:

```csharp
Patient? patient =
    await _repository.GetPatientByIdAsync(
        patientId,
        clinicId
    );
```

The new record receives its clinic from the trusted identity:

```csharp
IntakeRequest request = new()
{
    PatientId = patient.Id,
    ClinicId = clinicId,
};
```

### Repository

The repository places the clinic condition directly in the database query:

```csharp
return await _db
    .IntakeRequests
    .FirstOrDefaultAsync(request =>
        request.Id == id
        && request.ClinicId == clinicId
    );
```

Collection queries are filtered before searching, sorting, and pagination:

```csharp
return await _db
    .IntakeRequests
    .Where(request =>
        request.ClinicId == clinicId
    )
    .Include(request => request.Patient)
    .Include(request => request.Clinic)
    .ToListAsync();
```

This is the real data boundary. Filtering only in the controller would be easy to forget on a new endpoint. Filtering after pagination would also produce incorrect totals and partially empty pages.

## Read Isolation

For a single record, both the resource ID and clinic ID must match:

```text
ID matches + ClinicId matches     -> Return record
ID matches + ClinicId differs     -> Not found
ID does not match                 -> Not found
```

A foreign record and a nonexistent record deliberately produce the same result. This prevents one clinic from discovering which IDs exist for another clinic.

For a collection, the repository returns only records owned by the authenticated clinic. All later filtering and pagination operate on that already-isolated collection.

## Create Isolation

The POST body contains a `PatientId`, but not a trusted clinic identity.

```text
Clinic 1 token + Clinic 1 patient -> Create request
Clinic 1 token + Clinic 2 patient -> Patient not found
```

The repository searches for the patient with both conditions:

```csharp
patient.Id == patientId
&& patient.ClinicId == clinicId
```

This prevents a caller from creating a request for a patient belonging to another clinic.

## Update Isolation

Before updating status, the service retrieves the request using both its ID and the authenticated clinic ID.

If the request belongs to another clinic, the repository returns `null`, the service returns `false`, and the controller returns `404 Not Found`. The record remains unchanged.

The repository also refuses to save a request whose `ClinicId` does not match the trusted `clinicId` parameter:

```csharp
if (request.ClinicId != clinicId)
{
    return false;
}
```

This second check protects against mistakes in calling code.

## Delete Isolation

Deletion requires:

```text
Authenticated identity
    + valid ClinicId
    + Admin role
    + matching record ClinicId
```

An Admin belongs to a clinic. The role does not automatically grant access to every tenant.

The delete query searches within the clinic boundary. If the record belongs to another clinic, the API returns `404` and leaves the record intact.

## Seeder Behavior

The database seeder has no JWT because it runs during application startup. It is trusted maintenance code rather than a user request.

When seeding a patient request, it explicitly passes the patient's stored clinic ID:

```csharp
await intakeService.AddRequestAsync(
    patient.Id,
    patient.ClinicId
);
```

The seeder uses the `DbContext` directly when it must answer a global maintenance question such as whether any requests exist across all clinics. Passing one clinic to a scoped service method would answer a different question.

## In-Memory and EF Repositories

Every implementation of `IIntakeRepository` follows the same clinic-aware contract.

The EF Core repository applies `WHERE ClinicId = ...` in database queries. The in-memory repository applies equivalent LINQ filters to its lists. Keeping the contract consistent prevents development or test implementations from silently bypassing security behavior.

## Integration Tests

The integration suite tests the complete HTTP path for two different clinic identities.

It proves that a foreign clinic cannot:

- See another clinic's request in a collection
- Retrieve another clinic's request by ID
- Create a request for another clinic's patient
- Update another clinic's request
- Delete another clinic's request

Mutation tests verify both the HTTP status and the saved database state. A correct-looking error response is not sufficient if the database was still changed.

## Common Failure Modes

### Accepting ClinicId from the client

```json
{
  "patientId": 12,
  "clinicId": 2
}
```

The caller controls this JSON and could simply request another clinic. Clinic identity must come from the validated token.

### Filtering only collection endpoints

Securing `GET /requests` does not secure `GET /requests/{id}`, update, or delete operations. Every access path must carry the clinic boundary.

### Filtering after pagination

Paginating all tenants and filtering afterward leaks incorrect counts and produces incomplete pages. Tenant filtering must happen first.

### Treating Admin as global

Role authorization and tenant authorization answer different questions. An Admin can still be limited to its own clinic.

### Returning Forbidden for foreign records

A `403 Forbidden` response confirms that the requested record exists. Returning `404 Not Found` avoids that information leak.

## Key Lesson

Multi-tenant isolation is not one attribute or one `Where()` call. It is a value that must remain attached to the request through every layer:

```text
Validated identity
    -> authorization policy
    -> controller parameter
    -> service contract
    -> repository query
    -> verified database result
```

If any layer drops the clinic boundary, that operation becomes a potential cross-tenant data leak.

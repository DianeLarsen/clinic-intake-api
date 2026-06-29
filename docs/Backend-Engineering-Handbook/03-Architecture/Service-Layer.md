````markdown
# Service Layer

## What Problem Does This Solve?

The application needs a place for business logic.

Without a service layer, logic can end up scattered across endpoints, repositories, and models. That makes the application harder to understand, test, and change.

For example, the endpoint should not be responsible for deciding how to create an intake request, update a status, filter results, or map models into DTOs. That would turn `Program.cs` into a junk drawer with HTTP sprinkled on top. Humanity has suffered enough.

## Solution

A service layer contains application logic.

The endpoint receives the HTTP request.

The service decides what the application should do.

The repository handles data access.

```text
Endpoint
    ↓
Service
    ↓
Repository
````

## Where It Fits

In this project, `IntakeService` sits between the Minimal API endpoints and the repository.

```text
Endpoint
    ↓
IntakeService
    ↓
IIntakeRepository
    ↓
EfIntakeRepository
    ↓
DbContext
    ↓
SQLite Database
```

The endpoint should stay thin.

The service should contain the application logic.

The repository should handle storage.

## Why This Matters

* Keeps endpoints simple.
* Keeps business logic in one place.
* Makes the application easier to test.
* Prevents the repository from becoming responsible for application rules.
* Makes future changes easier.
* Helps separate HTTP concerns from application behavior.

## Mental Model

Think of the service layer as the manager.

The endpoint says:

```text
A client wants to create a request.
```

The service decides:

```text
Is the patient name valid?
Create the IntakeRequest.
Ask the repository to save it.
Return the result.
```

The repository does not decide the business process.

The endpoint does not decide the business process.

The service does.

## Real-World Example

In a clinic, the front desk receives a request.

The front desk does not personally manage every policy, file every record, and update every system.

Instead, the request follows a process.

The service layer is like that process coordinator.

It knows what should happen.

The records system stores the data.

## Code Example

The service depends on the repository interface:

```csharp
public class IntakeService : IIntakeService
{
    private readonly IIntakeRepository _repository;

    public IntakeService(IIntakeRepository repository)
    {
        _repository = repository;
    }
}
```

Creating a request:

```csharp
public async Task<IntakeRequest> AddRequestAsync(string patientName)
{
    if (string.IsNullOrWhiteSpace(patientName))
    {
        throw new ArgumentException("Patient name is required.");
    }

    IntakeRequest request = new IntakeRequest(patientName);

    return await _repository.AddAsync(request);
}
```

Filtering, sorting, pagination, and DTO mapping also belong in the service:

```csharp
public async Task<IEnumerable<RequestSummaryDto>> GetRequestSummariesAsync(
    RequestStatus? status,
    string? patient,
    string? sort,
    int page,
    int pageSize)
{
    IEnumerable<IntakeRequest> requests =
        await GetRequestsAsync(status, patient, sort, page, pageSize);

    return requests.Select(r => new RequestSummaryDto
    {
        Id = r.Id,
        DisplayText = $"{r.PatientName} - {r.Status}",
    });
}
```

The endpoint can stay simple:

```csharp
app.MapGet(
    "/requests",
    async (
        IIntakeService intakeService,
        RequestStatus? status,
        string? patient,
        string? sort,
        int page = 1,
        int pageSize = 10
    ) =>
    {
        return Results.Ok(
            await intakeService.GetRequestSummariesAsync(
                status,
                patient,
                sort,
                page,
                pageSize));
    }
);
```

## Common Beginner Questions

### Is a service the same as a repository?

No.

A repository handles data access.

A service handles application logic.

```text
Service = what should happen
Repository = how data is saved or loaded
```

### Why not put all logic in the endpoint?

Because endpoints should mostly handle HTTP concerns:

* Route
* Query parameters
* Request body
* Status codes
* Calling the service

If endpoints contain too much logic, they become harder to test and maintain.

### Why not put all logic in the repository?

Because repositories should focus on data access.

If business rules go into the repository, the data layer becomes responsible for decisions it should not own.

### Does every application need a service layer?

No.

Tiny applications may not need one.

But once logic grows beyond simple CRUD, a service layer helps keep the application organized.

## Common Mistakes

* Putting HTTP response logic inside services.
* Putting database-specific code inside services.
* Putting business rules inside repositories.
* Making services too thin to be useful.
* Making services too large by mixing unrelated responsibilities.

## Interview Answer

A service layer contains application and business logic. It sits between the API layer and the data access layer, keeping endpoints thin and repositories focused on storage. This improves organization, testability, and maintainability.

## One-Sentence Summary

The service layer is where application logic belongs, keeping endpoints simple and repositories focused on data access.

## What Finally Made It Click

* The endpoint handles HTTP.
* The service decides what should happen.
* The repository handles data storage.
* DTO mapping belongs in the service, not the endpoint.
* A boring endpoint is usually a good endpoint.

```
```

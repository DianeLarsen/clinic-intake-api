## DTOs + Select()

DTOs become especially useful when combined with LINQ's `Select()` method.

`Select()` transforms one type of object into another.

In this project, the repository returns `IntakeRequest` objects, but the API does not always need to expose the full model.

Instead, the service can transform each `IntakeRequest` into a `RequestSummaryDto`.

```csharp
public IEnumerable<RequestSummaryDto> GetRequestSummaries(
    RequestStatus? status,
    string? patient,
    string? sort,
    int page,
    int pageSize)
{
    IEnumerable<IntakeRequest> requests =
        GetRequests(status, patient, sort, page, pageSize);

    return requests.Select(r => new RequestSummaryDto
    {
        Id = r.Id,
        DisplayText = $"{r.PatientName} - {r.Status}",
    });
}

### The flow is:

Repository
↓
IntakeRequest
↓
Service
↓
Select()
↓
RequestSummaryDto
↓
Endpoint
↓
JSON Response

This keeps the endpoint simple and moves data-shaping logic into the service.

### Why This Matters

* Keeps API responses focused.
* Prevents exposing full internal models unnecessarily.
* Allows different endpoints to return different shapes of data.
* Keeps endpoints thin and services responsible for application logic.
* Makes response models easier to change without changing database models.

### Mental Model

The model is the internal object.

The DTO is the package sent outside the application.

Select() is the packing step.

Model + Select() = DTO

### One-Sentence Summary

Select() transforms internal models into DTOs so the API can return only the data clients actually need.
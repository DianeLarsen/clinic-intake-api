# API Versioning

## What is API Versioning?

API versioning allows an application to support more than one version of an API.

An API may need a new version when its routes, request bodies, response bodies, or behavior change in a way that could break existing clients.

Without versioning, changing an endpoint can cause older applications to stop working.

Example:

```text
/api/v1/requests
/api/v2/requests
```

Version 1 can continue working while version 2 introduces new behavior.

---

## Why Version an API?

Suppose version 1 returns:

```json
{
  "id": 1,
  "patientName": "Diane Larsen"
}
```

Later, the API needs to return:

```json
{
  "id": 1,
  "patient": {
    "firstName": "Diane",
    "lastName": "Larsen"
  }
}
```

Changing the original response could break clients that expect `patientName`.

Instead, the application can keep version 1 and create version 2.

```text
GET /api/v1/requests
GET /api/v2/requests
```

---

## URL Versioning

The Clinic Intake API uses the version number in the URL.

Example:

```text
GET /api/v1/requests
```

The version is easy to see because it is part of the route.

Other APIs may use headers or query strings, but URL versioning is simple and clear for learning projects.

---

## Required Packages

The application uses these packages:

```bash
dotnet add ClinicIntakeApi.csproj package Asp.Versioning.Mvc --version 8.0.0
```

```bash
dotnet add ClinicIntakeApi.csproj package Asp.Versioning.Mvc.ApiExplorer --version 8.0.0
```

The package major version should match the application's .NET version.

For this project:

```text
.NET 8
    ↓
Asp.Versioning 8.x
```

---

## Registering API Versioning

Add the namespace to `Program.cs`:

```csharp
using Asp.Versioning;
```

Then register API versioning:

```csharp
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);

        options.AssumeDefaultVersionWhenUnspecified = true;

        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";

        options.SubstituteApiVersionInUrl = true;
    });
```

---

## `DefaultApiVersion`

```csharp
options.DefaultApiVersion = new ApiVersion(1, 0);
```

This sets the default version to:

```text
1.0
```

The first number is the major version.

The second number is the minor version.

```text
1.0
│ │
│ └── Minor version
└──── Major version
```

---

## `AssumeDefaultVersionWhenUnspecified`

```csharp
options.AssumeDefaultVersionWhenUnspecified = true;
```

This tells ASP.NET to use the default version when a client does not provide one.

However, when the version is part of the route, clients should normally use the full versioned URL:

```text
/api/v1/requests
```

---

## `ReportApiVersions`

```csharp
options.ReportApiVersions = true;
```

This tells ASP.NET to include supported API versions in response headers.

This helps clients see which versions are available.

---

## Swagger API Explorer

Swagger needs extra configuration so it understands versioned routes.

```csharp
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";

    options.SubstituteApiVersionInUrl = true;
});
```

---

## `GroupNameFormat`

```csharp
options.GroupNameFormat = "'v'VVV";
```

This controls how the API version is displayed.

For version `1.0`, the group name becomes:

```text
v1
```

---

## `SubstituteApiVersionInUrl`

```csharp
options.SubstituteApiVersionInUrl = true;
```

Without this setting, Swagger may display:

```text
/api/v{version}/requests
```

With this setting, Swagger displays:

```text
/api/v1/requests
```

This replaces the route placeholder with the actual API version.

---

## Versioning a Controller

Add the version attribute to the controller:

```csharp
[ApiVersion(1.0)]
```

Update the controller route:

```csharp
[Route("api/v{version:apiVersion}/[controller]")]
```

Example:

```csharp
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class RequestsController : ControllerBase
{
}
```

---

## Understanding the Route

This route:

```csharp
[Route("api/v{version:apiVersion}/[controller]")]
```

contains several parts.

```text
api
```

Groups the route under the API path.

```text
v{version:apiVersion}
```

Places the version number in the URL.

```text
[controller]
```

Uses the controller name without the word `Controller`.

For:

```text
RequestsController
```

the route becomes:

```text
/api/v1/requests
```

---

## Versioned Clinic Intake Routes

The version 1 routes are:

```text
GET    /api/v1/requests
GET    /api/v1/requests/{id}
POST   /api/v1/requests
PUT    /api/v1/requests/{id}/status
DELETE /api/v1/requests/{id}
```

---

## Updating Integration Tests

Integration tests must use the same routes as the real API.

Old route:

```csharp
await _client.GetAsync("/requests");
```

Versioned route:

```csharp
await _client.GetAsync("/api/v1/requests");
```

POST example:

```csharp
await _client.PostAsJsonAsync(
    "/api/v1/requests",
    dto
);
```

PUT example:

```csharp
await _client.PutAsJsonAsync(
    $"/api/v1/requests/{requestId}/status",
    dto,
    JsonOptions
);
```

DELETE example:

```csharp
await _client.DeleteAsync(
    $"/api/v1/requests/{requestId}"
);
```

Unit tests usually do not need route changes because they test services directly and do not send HTTP requests.

---

## When Should a New Version Be Created?

A new major API version may be needed when a change would break existing clients.

Examples:

- Removing a response field
- Renaming a response field
- Changing a request body
- Changing route behavior
- Changing the meaning of a status code
- Replacing one data structure with another

A new version is usually not needed for changes that do not break clients.

Examples:

- Fixing a bug
- Improving performance
- Adding an optional field
- Adding a new endpoint

---

## Example Version 2 Controller

A future version could look like:

```csharp
[ApiController]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class RequestsController : ControllerBase
{
}
```

The route would become:

```text
/api/v2/requests
```

Version 1 and version 2 could exist at the same time.

---

## Important Idea

API versioning protects existing clients from breaking changes.

```text
Old clients
    ↓
/api/v1/requests

New clients
    ↓
/api/v2/requests
```

This allows the API to improve without forcing every client to update immediately.
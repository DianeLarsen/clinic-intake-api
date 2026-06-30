# Validation

## What Problem Does This Solve?

Applications cannot assume every request contains valid data.

For example:

- A required field may be missing.
- A name may be too long.
- A number may be outside an acceptable range.
- An email address may not be valid.

Without validation, invalid data reaches the service layer, making the application more difficult to maintain and increasing the risk of errors.

## Solution

Validation checks incoming data before business logic executes.

In ASP.NET Core, validation rules are typically defined using **Data Annotations** on DTOs.

Example:

```csharp
using System.ComponentModel.DataAnnotations;

public class CreateRequestDto
{
    [Required]
    [StringLength(100)]
    public string PatientName { get; set; } = "";
}
```

These attributes describe the rules for valid data.

## Why This Matters

Validation:

- Prevents invalid data from entering the application.
- Keeps business logic focused on business rules.
- Produces consistent error responses.
- Eliminates repetitive validation code.
- Makes DTOs self-documenting.

A well-designed API rejects bad requests as early as possible.

## Mental Model

Think of validation as a security checkpoint.

```text
HTTP Request
      │
      ▼
Validation
      │
      ▼
Business Logic
      │
      ▼
Database
```

If validation fails, the request never reaches the service layer.

## Data Annotations

Data Annotations describe validation rules.

Common examples include:

### Required

```csharp
[Required]
```

The value must be supplied.

---

### StringLength

```csharp
[StringLength(100)]
```

Limits the maximum length of a string.

---

### MinLength

```csharp
[MinLength(2)]
```

Requires a minimum number of characters.

---

### MaxLength

```csharp
[MaxLength(100)]
```

Limits the maximum size of a property.

---

### Range

```csharp
[Range(1, 100)]
```

Restricts numeric values.

---

### EmailAddress

```csharp
[EmailAddress]
```

Requires a valid email format.

---

### Phone

```csharp
[Phone]
```

Requires a valid phone number.

## Real-World Example

Instead of writing validation inside every endpoint:

```csharp
if (string.IsNullOrWhiteSpace(dto.PatientName))
{
    return Results.BadRequest(
        "Patient name is required.");
}
```

the rules become part of the DTO.

```csharp
public class CreateRequestDto
{
    [Required]
    [StringLength(100)]
    public string PatientName { get; set; } = "";
}
```

The endpoint becomes much cleaner.

```csharp
IntakeRequest request =
    await intakeService.AddRequestAsync(
        dto.PatientName);
```

The endpoint focuses on business logic rather than input validation.

## Validation in Controllers

When using MVC Controllers with the `[ApiController]` attribute, ASP.NET Core automatically validates DTOs.

If validation fails:

```http
400 Bad Request
```

is returned before the controller action executes.

Developers often don't need to write any validation code.

## Validation in Minimal APIs

Minimal APIs are intentionally lightweight.

They do not automatically validate Data Annotation attributes.

Instead, developers typically:

- Validate manually.
- Use an Endpoint Filter.
- Use a validation library such as FluentValidation.

In this project, validation is handled using an Endpoint Filter.

## Endpoint Filters

Endpoint Filters allow common logic to run before an endpoint executes.

```text
HTTP Request
      │
      ▼
Endpoint Filter
      │
      ▼
Endpoint
      │
      ▼
Service
```

A validation filter can:

- Read the DTO.
- Execute validation.
- Return a `400 Bad Request` if validation fails.
- Skip the endpoint entirely.

This keeps endpoints focused on business logic.

## Common Validation Flow

```text
Client Request
        │
        ▼
Model Binding
        │
        ▼
Validation
        │
        ▼
Valid?
     /      \
   Yes      No
    │        │
    ▼        ▼
Endpoint   400 Bad Request
```

## Common Beginner Questions

### Why not validate inside the service?

Services should assume they receive valid data.

Validation belongs at the boundary of the application.

The API should reject invalid requests before business logic begins.

---

### Why put validation on the DTO?

The DTO represents the incoming request.

By attaching validation rules to the DTO, every endpoint using that DTO automatically follows the same rules.

---

### Are Data Annotations enough?

For simple validation, yes.

More complex business rules often require custom validation or libraries such as FluentValidation.

---

### Why use an Endpoint Filter?

Without a filter, every endpoint would repeat the same validation code.

Endpoint Filters centralize validation so it is written once and reused everywhere.

## Common Mistakes

- Validating the same rules in every endpoint.
- Mixing validation with business logic.
- Assuming Minimal APIs automatically validate Data Annotations.
- Forgetting that validation should occur before database operations.
- Returning inconsistent validation error responses.

## Interview Answer

Validation ensures incoming requests contain acceptable data before business logic executes. In ASP.NET Core, validation rules are commonly defined using Data Annotations on DTOs. Controllers perform validation automatically, while Minimal APIs typically use Endpoint Filters or custom validation logic.

## One-Sentence Summary

Validation protects the application by ensuring only valid data reaches the business logic.

## What Finally Made It Click

- Validation is not business logic.
- Validation belongs at the edge of the application.
- DTOs define the rules.
- Endpoint Filters enforce the rules.
- Services should receive already validated data.
- Data Annotations don't perform validation by themselves. They describe the rules, and ASP.NET (or an Endpoint Filter) reads those rules and enforces them.
- Controllers automatically validate Data Annotations, but Minimal APIs require you to opt into validation, which explains why my original `if (string.IsNullOrWhiteSpace(...))` checks worked but became unnecessary once validation was centralized.

## Related Topics

### Previous

- DTOs
- Minimal APIs
- API Responses

### Next

- Controllers
- Middleware
- Endpoint Filters

### See Also

- Dependency Injection
- Service Layer
- Repository Pattern
- Nullable Reference Types
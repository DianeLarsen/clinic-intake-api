# Validation

## What is Validation?

Validation checks whether incoming data is acceptable before the application tries to use it.

Example:

```json
{
  "patientId": -5
}
```

A negative patient ID does not make sense.

Validation should reject that request before it reaches the service or database.

---

## Request Flow with Validation

```text
HTTP Request
    ↓
JSON is converted into a DTO
    ↓
Validation rules are checked
    ↓
Invalid → 400 Bad Request
    ↓
Valid → Controller runs
```

Because the controller uses:

```csharp
[ApiController]
```

ASP.NET automatically checks validation attributes on DTOs.

If validation fails, ASP.NET returns `400 Bad Request` before the controller action runs.

---

## Data Annotation Attributes

Validation rules can be added with attributes from:

```csharp
using System.ComponentModel.DataAnnotations;
```

Common attributes include:

```csharp
[Required]
[Range]
[StringLength]
[MinLength]
[MaxLength]
[EmailAddress]
[EnumDataType]
```

---

# Validating Numbers with `[Range]`

Example:

```csharp
public class CreateRequestDto
{
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "PatientId must be greater than 0."
    )]
    public int PatientId { get; set; }
}
```

This rule means:

```text
PatientId must be at least 1.
```

Valid values:

```text
1
25
900
```

Invalid values:

```text
0
-1
-500
```

---

## Why `[Required]` Is Not Enough for `int`

An `int` is a non-nullable value type.

If the client sends:

```json
{}
```

ASP.NET creates:

```csharp
PatientId = 0;
```

The value is not missing. It has the default value `0`.

Because of this:

```csharp
[Required]
public int PatientId { get; set; }
```

does not prevent `0`.

Use `[Range]` instead:

```csharp
[Range(1, int.MaxValue)]
public int PatientId { get; set; }
```

---

## Nullable Values and `[Required]`

If the property is nullable:

```csharp
public int? PatientId { get; set; }
```

then `[Required]` becomes useful:

```csharp
[Required]
public int? PatientId { get; set; }
```

Now the property can truly be missing or `null`.

---

# Validating Enums

Example enum:

```csharp
public enum RequestStatus
{
    Submitted,
    InReview,
    Completed
}
```

DTO validation:

```csharp
public class UpdateRequestStatusDto
{
    [EnumDataType(typeof(RequestStatus))]
    public RequestStatus Status { get; set; }
}
```

This tells ASP.NET that `Status` must contain a valid `RequestStatus` value.

Valid:

```json
{
  "status": "Submitted"
}
```

```json
{
  "status": "InReview"
}
```

```json
{
  "status": "Completed"
}
```

Invalid:

```json
{
  "status": "Banana"
}
```

```json
{
  "status": 999
}
```

---

# Automatic Validation Responses

When validation fails, ASP.NET returns a response similar to:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "PatientId": [
      "PatientId must be greater than 0."
    ]
  },
  "traceId": "..."
}
```

The controller action does not run.

---

# Validation vs Business Rules

Validation and business rules are related, but they are not the same.

## Validation

Validation checks whether the request has an acceptable shape.

Example:

```text
PatientId = -1
```

This is invalid because IDs must be greater than zero.

The validation layer rejects it.

---

## Business Rule

A business rule checks whether the request makes sense inside the application.

Example:

```text
PatientId = 999999
```

This is a valid positive number, but no patient with that ID exists.

The service checks the database and returns `null`.

The controller then returns:

```text
400 Bad Request
```

So:

```text
-1
→ Validation failure

999999
→ Valid input shape
→ Patient not found
→ Business-rule failure
```

---

# Example Controller

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateRequestDto dto)
{
    IntakeRequest? request =
        await _intakeService.AddRequestAsync(dto.PatientId);

    if (request is null)
    {
        return BadRequest(
            $"Patient with ID {dto.PatientId} does not exist."
        );
    }

    return Created(
        $"/api/v1/requests/{request.Id}",
        new IntakeRequestResponseDto
        {
            Id = request.Id,
            PatientId = request.PatientId,
            ClinicId = request.ClinicId,
            Status = request.Status,
        }
    );
}
```

The controller does not manually check:

```csharp
if (dto.PatientId <= 0)
```

ASP.NET already handled that through `[Range]`.

---

# Integration Testing Validation

## Negative Patient ID

```csharp
[Fact]
public async Task CreateRequest_WhenPatientIdIsNegative_ReturnsValidationError()
{
    // Arrange
    var dto = new CreateRequestDto
    {
        PatientId = -1
    };

    // Act
    HttpResponseMessage response =
        await _client.PostAsJsonAsync(
            "/api/v1/requests",
            dto
        );

    string responseBody =
        await response.Content.ReadAsStringAsync();

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode
    );

    Assert.Contains(
        "PatientId must be greater than 0.",
        responseBody
    );
}
```

---

## Positive but Missing Patient

```csharp
[Fact]
public async Task CreateRequest_WhenPatientDoesNotExist_ReturnsBadRequest()
{
    // Arrange
    var dto = new CreateRequestDto
    {
        PatientId = 999999
    };

    // Act
    HttpResponseMessage response =
        await _client.PostAsJsonAsync(
            "/api/v1/requests",
            dto
        );

    string responseBody =
        await response.Content.ReadAsStringAsync();

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode
    );

    Assert.Contains(
        "does not exist",
        responseBody
    );
}
```

---

## Invalid Enum Value

```csharp
[Fact]
public async Task UpdateStatus_WhenStatusIsInvalid_ReturnsBadRequest()
{
    // Arrange
    int requestId = 1;

    var dto = new UpdateRequestStatusDto
    {
        Status = (RequestStatus)999
    };

    // Act
    HttpResponseMessage response =
        await _client.PutAsJsonAsync(
            $"/api/v1/requests/{requestId}/status",
            dto,
            JsonOptions
        );

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode
    );
}
```

---

# Common Validation Attributes

## Required

```csharp
[Required]
public string FirstName { get; set; } = string.Empty;
```

Use when a value must be present.

---

## Range

```csharp
[Range(1, 100)]
public int Quantity { get; set; }
```

Use when a number must fall within a certain range.

---

## StringLength

```csharp
[StringLength(
    100,
    MinimumLength = 2
)]
public string Name { get; set; } = string.Empty;
```

Use when text must have a minimum or maximum length.

---

## EmailAddress

```csharp
[EmailAddress]
public string Email { get; set; } = string.Empty;
```

Use for basic email-format validation.

---

## EnumDataType

```csharp
[EnumDataType(typeof(RequestStatus))]
public RequestStatus Status { get; set; }
```

Use when a value must belong to an enum.

---

# Important Idea

Validation protects the application before bad data reaches business logic or the database.

```text
Request
    ↓
DTO
    ↓
Validation
    ↓
Controller
    ↓
Service
    ↓
Repository
    ↓
Database
```

Good validation rejects bad input early and returns clear error messages.
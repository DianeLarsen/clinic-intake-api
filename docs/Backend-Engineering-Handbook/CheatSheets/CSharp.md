# C# Cheat Sheet

## Variables

```csharp
int count = 10;
string name = "Diane";
bool completed = true;
```

---

## Nullable Types

```csharp
string? patientName = null;

IntakeRequest? request =
    await FindRequestByIdAsync(id);
```

---

## Object Creation

```csharp
IntakeRequest request =
    new IntakeRequest("Diane");
```

---

## Object Initializers

```csharp
new RequestSummaryDto
{
    Id = request.Id,
    DisplayText = $"{request.PatientName} - {request.Status}"
};
```

---

## Properties

```csharp
public string PatientName { get; set; } = "";
```

Read-only property:

```csharp
public int Id { get; private set; }
```

---

## Constructors

```csharp
public IntakeService(IIntakeRepository repository)
{
    _repository = repository;
}
```

---

## Methods

```csharp
public int GetCount()
{
    return 5;
}
```

Async method:

```csharp
public async Task<int> GetCountAsync()
{
    return 5;
}
```

---

## If Statements

```csharp
if (status is not null)
{
}
```

```csharp
if (!string.IsNullOrWhiteSpace(patient))
{
}
```

---

## Switch Expression

```csharp
status switch
{
    RequestStatus.Submitted => "...",
    RequestStatus.Completed => "...",
    _ => "Unknown"
};
```

---

## Collections

List

```csharp
List<string> names = [];
```

Array

```csharp
string[] names =
[
    "Alice",
    "Bob",
    "Charlie"
];
```

IEnumerable

```csharp
IEnumerable<IntakeRequest> requests;
```

---

## Generics

```csharp
List<IntakeRequest>

Task<IntakeRequest>

IEnumerable<RequestSummaryDto>

DbSet<IntakeRequest>

PagedResponse<RequestSummaryDto>
```

Mental model:

```text
Container<Thing>
```

---

## Async / Await

Method

```csharp
public async Task<int> GetCountAsync()
{
}
```

Await

```csharp
var requests =
    await _repository.GetAllAsync();
```

---

## LINQ

```csharp
Where()

Select()

Count()

FirstOrDefault()

Any()

Contains()

OrderBy()

OrderByDescending()

Skip()

Take()
```

---

## Lambda Expressions

```csharp
r => r.PatientName

r => r.Status == RequestStatus.Completed
```

General form:

```csharp
parameter => expression
```

---

## String Interpolation

```csharp
$"{request.PatientName} - {request.Status}"
```

---

## Dependency Injection

Constructor Injection

```csharp
private readonly IIntakeRepository _repository;

public IntakeService(
    IIntakeRepository repository)
{
    _repository = repository;
}
```

---

## Common Return Types

```csharp
void

bool

int

string

Task

Task<T>

IEnumerable<T>

PagedResponse<T>
```

---

## Common Keywords

| Keyword | Purpose |
|----------|---------|
| `public` | Visible everywhere |
| `private` | Visible only inside class |
| `readonly` | Cannot change after construction |
| `async` | Method contains awaits |
| `await` | Wait for async work |
| `return` | Return a value |
| `new` | Create an object |
| `var` | Compiler infers the type |

---

## Naming Conventions

| Item | Style | Example |
|------|-------|---------|
| Class | PascalCase | `IntakeService` |
| Interface | PascalCase with I | `IRepository` |
| Method | PascalCase | `GetRequestsAsync()` |
| Property | PascalCase | `PatientName` |
| Private field | `_camelCase` | `_repository` |
| Local variable | camelCase | `request` |
| Parameter | camelCase | `patientName` |

---

## File Organization

```text
using statements

namespace

class

fields

constructor

public methods

private methods
```

---

## Things I Use Constantly

```csharp
await

Task<T>

IEnumerable<T>

List<T>

DbSet<T>

Where()

Select()

Any()

Count()

OrderBy()

Skip()

Take()

FirstOrDefault()

string.IsNullOrWhiteSpace()

Math.Ceiling()

Results.Ok()

Results.NotFound()

Results.Created()

Results.NoContent()
```
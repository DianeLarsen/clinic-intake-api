# Mocking

## What is Mocking?

Mocking means creating a fake version of an object.

Instead of using the real database or repository, tests use a fake object that behaves in predictable ways.

Example:

```text
Real Application

Service
    ↓
Repository
    ↓
Database
```

Unit tests replace the repository:

```text
Unit Test

Service
    ↓
Mock Repository
```

---

## Creating a Mock

Moq creates fake objects.

Example:

```csharp
var repositoryMock = new Mock<IIntakeRepository>();
```

This creates a fake repository.

---

## `.Setup()`

`.Setup()` teaches the mock how to behave.

Example:

```csharp
repositoryMock
    .Setup(repository => repository.GetPatientByIdAsync(1))
    .ReturnsAsync(patient);
```

Meaning:

> "When someone asks for patient 1, return this patient."

---

## `.ReturnsAsync()`

`.ReturnsAsync()` defines the value that the mock should return.

Example:

```csharp
.ReturnsAsync(patient);
```

Example:

```csharp
.ReturnsAsync((Patient?)null);
```

Meaning:

> "Pretend the patient does not exist."

---

## `.Verify()`

`.Verify()` checks whether a method was called.

Example:

```csharp
repositoryMock.Verify(
    repository => repository.DeleteAsync(123),
    Times.Once
);
```

Meaning:

> "Make sure `DeleteAsync(123)` was called exactly one time."

Common options:

```csharp
Times.Once
Times.Never
Times.Exactly(3)
Times.AtLeastOnce()
```

---

## Why Mock Repositories?

Repositories:

- Talk to databases.
- Use Entity Framework.
- Depend on external systems.
- Make tests slower.

Mocking allows tests to focus only on the service logic.

---

## Why Not Mock Models or DTOs?

Models and DTOs are usually simple objects that only hold data.

Example:

```csharp
var patient = new Patient
{
    Id = 1,
    FirstName = "Diane",
    LastName = "Larsen"
};
```

Simple data objects are easy to create and do not require mocking.

General rule:

- Repositories, databases, APIs, and email services are often mocked.
- Models and DTOs are usually created with `new`.

---

## Mocking Rule of Thumb

Mock objects that perform work.

Use real objects for simple data.

```text
Mock:

Repository
Database
Email service
External API

Use real objects:

Patient
IntakeRequest
DTOs
```


## Setup vs ReturnsAsync

`.Setup()` defines which method call is being intercepted.

```csharp
repositoryMock
    .Setup(r => r.GetByIdAsync(123))


.ReturnsAsync() defines what value should be returned.

.ReturnsAsync(request);

Together:

repositoryMock
    .Setup(r => r.GetByIdAsync(123))
    .ReturnsAsync(request);

means:

"When someone asks for request 123, return this request."


And add:

```md
## Verify

`Verify()` checks that a method was called.

```csharp
repositoryMock.Verify(
    repository => repository.UpdateAsync(request),
    Times.Once
);
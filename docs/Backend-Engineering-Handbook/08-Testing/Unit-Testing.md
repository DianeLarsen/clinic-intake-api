# Unit Testing

## What is Unit Testing?

A unit test checks that one small piece of code behaves correctly.

Usually, a unit test focuses on a single class or method.

For example:

```text
IntakeService
```

instead of:

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
Database
```

The goal is to test one piece of code in isolation.

---

## Arrange, Act, Assert

Most unit tests follow the same three-step pattern.

### Arrange

Prepare everything needed for the test.

Examples:

- Create objects.
- Create mock dependencies.
- Configure test data.

```csharp
var repositoryMock = new Mock<IIntakeRepository>();
```

---

### Act

Run the code being tested.

```csharp
IntakeRequest? result = await service.AddRequestAsync(1);
```

---

### Assert

Verify the result.

```csharp
Assert.NotNull(result);
Assert.Equal(1, result.PatientId);
```

---

## Example

```csharp
[Fact]
public async Task AddRequestAsync_WhenPatientExists_ReturnsNewRequest()
{
    // Arrange

    var repositoryMock = new Mock<IIntakeRepository>();

    var service = new IntakeService(repositoryMock.Object);

    // Act

    IntakeRequest? result = await service.AddRequestAsync(1);

    // Assert

    Assert.NotNull(result);
}
```

---

# Unit Tests vs Integration Tests

## Unit Test

Tests a single component.

```text
Service
    ↓
Fake Repository
```

Questions answered:

- Did the service return the correct result?
- Did the service apply business rules correctly?

Unit tests are:

- Fast
- Small
- Focused

---

## Integration Test

Tests multiple components working together.

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
Database
```

Questions answered:

- Did the database update correctly?
- Did the API endpoint work?
- Did all layers communicate properly?

Integration tests are:

- Slower
- Larger
- Closer to real-world behavior

---

## What Makes a Good Unit Test?

Good unit tests:

- Test one behavior.
- Have clear names.
- Use simple test data.
- Run quickly.
- Avoid unnecessary setup.
- Verify business logic.

Good test names read like sentences:

```text
AddRequestAsync_WhenPatientExists_ReturnsNewRequest

UpdateStatusAsync_WhenRequestDoesNotExist_ReturnsFalse
```


```md
## Real objects vs mocked objects

Unit tests often create real model objects:

```csharp
var patient = new Patient
{
    Id = 1,
    FirstName = "Diane"
};

Models are simple data containers and usually do not need to be mocked.

Dependencies such as repositories are mocked because they access external systems like databases.

Example:

var repositoryMock =
    new Mock<IIntakeRepository>();
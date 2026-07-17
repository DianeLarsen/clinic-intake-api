# Integration Testing

## What is integration testing?

Integration tests verify that multiple parts of the application work together.

Unlike unit tests, integration tests use the real:

- Controllers
- Services
- Repositories
- Entity Framework
- Database

Example:

HTTP Request
    ↓
Middleware
    ↓
Controller
    ↓
Service
    ↓
Repository
    ↓
SQLite

## Unit tests vs integration tests

Unit test:

Service
    ↓
Fake Repository

Integration test:

HTTP Request
    ↓
Controller
    ↓
Service
    ↓
Repository
    ↓
Database

## WebApplicationFactory

`WebApplicationFactory<Program>` starts a copy of the ASP.NET application inside the test runner.

Example:

```csharp
public class RequestsApiTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RequestsApiTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
}
```

## Program.cs Requirement

Because `Program.cs` uses top-level statements, integration tests need this at the bottom of `Program.cs`:

```csharp
public partial class Program { }
```

This lets `WebApplicationFactory<Program>` locate the application entry point.

## Common Integration-Test Flow

1. Arrange test data.
2. Send an HTTP request.
3. Read the HTTP response.
4. Verify the response and any database changes.

## Database Isolation

Integration tests must use a database created for testing, never the application's normal development database.

See [Isolated Integration-Test Databases](Isolated-Integration-Test-Databases.md) for the implementation used by this project.
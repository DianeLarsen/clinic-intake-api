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

Program.cs requirement

Because Program.cs uses top-level statements, tests need:

public partial class Program { }

at the bottom of Program.cs.

Common integration test flow
Arrange test data.
Send an HTTP request.
Read the response.
Verify the database changed.
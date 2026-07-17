# Isolated Integration-Test Databases

## Problem

Integration tests run the real ASP.NET Core application, including Entity Framework Core and SQLite.

The application normally uses this development database:

```text
clinic-intake.db
```

If integration tests use that same database, they can:

- Change or delete development data
- Depend on data left behind by a previous test run
- Pass locally but fail in a clean environment
- Interfere with another test run

Tests need their own database that is separate from local development data.

---

## Solution

`CustomWebApplicationFactory` creates a unique temporary SQLite file for each test factory.

```csharp
private readonly string _databasePath = Path.Combine(
    Path.GetTempPath(),
    $"clinic-intake-tests-{Guid.NewGuid():N}.db"
);
```

The generated GUID makes each filename unique.

Example:

```text
/tmp/clinic-intake-tests-a1b2c3d4e5f6.db
```

This keeps test data separate from the application's normal `clinic-intake.db` file.

---

## Replacing the Application Database Registration

`Program.cs` registers the normal application database:

```csharp
builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite(connectionString)
);
```

Inside the test factory, that registration is removed and replaced.

```csharp
services.RemoveAll<DbContextOptions<ClinicIntakeDbContext>>();

services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite($"Data Source={_databasePath}")
);
```

`RemoveAll` is important.

Without it, the test server could have both the development and test database registrations. That makes it unclear which database Entity Framework will use.

The replacement happens only inside `ConfigureTestServices`, so the normal application still uses its development configuration outside the test runner.

---

## Test Startup and Seeding

The test factory uses the `Testing` environment:

```csharp
builder.UseEnvironment("Testing");
```

When the test server starts, `Program.cs` still runs:

```csharp
await DbSeeder.SeedAsync(app.Services);
```

The seeder calls `EnsureCreatedAsync()`, which creates tables in the temporary database file.

It also creates the same sample clinics, patients, and intake requests used by the integration tests.

The test database is therefore:

- Empty before the factory starts
- Created and seeded when the test server starts
- Separate from development data
- Available to controllers, services, repositories, and health checks

---

## Database Cleanup

`CustomWebApplicationFactory` overrides `Dispose`.

```csharp
protected override void Dispose(bool disposing)
{
    base.Dispose(disposing);

    if (disposing && File.Exists(_databasePath))
    {
        File.Delete(_databasePath);
    }
}
```

The test server is disposed first so SQLite can release its open connections.

The temporary database file is then deleted.

This prevents temporary test files from accumulating after each test run.

---

## Verifying Isolation

`CustomWebApplicationFactoryTests` verifies that the factory creates a temporary database.

The test confirms that:

1. The file does not exist before the test server starts.
2. `/health/ready` returns `200 OK`.
3. Startup creates the database file.
4. The file exists in the system temporary directory.
5. The file is not named `clinic-intake.db`.

A second test confirms that disposing the factory deletes the temporary database file.

These tests protect the test infrastructure itself. If someone later removes the test database replacement or cleanup code, the automated tests will detect it.

---

## Fixture Scope and Per-Test Reset

Integration-test classes use:

```csharp
IClassFixture<CustomWebApplicationFactory>
```

This means one `CustomWebApplicationFactory` instance—and therefore one temporary database file—is shared by tests in the same class.

A shared fixture is efficient, but it creates a risk: one test can add, update, or delete data that another test later reads.

`RequestsApiTests` uses xUnit's `IAsyncLifetime` interface to reset the database before each test method.

```csharp
public class RequestsApiTests
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
```

The lifecycle for every test method is:

```text
Create test-class instance
        ↓
InitializeAsync resets and seeds database
        ↓
Run one test method
        ↓
DisposeAsync
        ↓
Next test starts from the same seeded baseline
```

---

## Resetting the Database

`CustomWebApplicationFactory` provides a reusable reset method.

```csharp
public async Task ResetDatabaseAsync()
{
    using (IServiceScope scope = Services.CreateScope())
    {
        ClinicIntakeDbContext db =
            scope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

        await db.Database.EnsureDeletedAsync();
    }

    await DbSeeder.SeedAsync(Services);
}
```

The reset process:

1. Deletes the temporary database tables and data.
2. Runs the normal application seeder again.
3. Recreates the schema and predictable sample records.
4. Leaves the development database untouched.

`CustomWebApplicationFactoryTests` verifies that data created before a reset does not exist afterward.

---

## Key Takeaways

- Integration tests should never use the development database.
- A unique temporary SQLite file gives each test factory an isolated database.
- `ConfigureTestServices` can replace production service registrations for the test server only.
- Startup seeding creates reliable baseline data.
- `IAsyncLifetime.InitializeAsync()` can reset shared fixture data before each test method.
- Tests should verify their test infrastructure, not only application endpoints.
- Disposing the factory removes temporary database files.
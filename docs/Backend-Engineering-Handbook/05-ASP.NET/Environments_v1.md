# Environments

## What is an Environment?

An environment tells ASP.NET where the application is running.

The same application can run in different environments with different settings.

Common environments:

- Development
- Testing
- Production

Example:

```text
Developer computer
        ↓
Development

Integration tests
        ↓
Testing

Azure server
        ↓
Production
```

Environments allow the application to change behavior without changing code.

---

# Why Environments Matter

Different environments often need different settings.

Examples:

| Environment | Database | Swagger | Logging |
| --- | --- | --- | --- |
| Development | SQLite | Enabled | Detailed |
| Testing | Test SQLite database | Disabled | Reduced |
| Production | Azure SQL | Disabled | Structured |

Without environments, developers would constantly edit code before deployment.

---

# The ASPNETCORE_ENVIRONMENT Variable

ASP.NET uses the `ASPNETCORE_ENVIRONMENT` environment variable.

Possible values:

```text
Development
Testing
Production
```

---

# Setting the Environment

## Linux / macOS

Development:

```bash
export ASPNETCORE_ENVIRONMENT=Development
```

Testing:

```bash
export ASPNETCORE_ENVIRONMENT=Testing
```

Production:

```bash
export ASPNETCORE_ENVIRONMENT=Production
```

Run the application:

```bash
dotnet run
```

---

## Check the Current Environment

Inside `Program.cs`:

```csharp
Console.WriteLine(app.Environment.EnvironmentName);
```

Example output:

```text
Development
```

---

# Built-In Environment Checks

ASP.NET provides helper methods.

Development:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
}
```

Production:

```csharp
if (app.Environment.IsProduction())
{
    // Production-only code
}
```

Custom environment:

```csharp
if (app.Environment.IsEnvironment("Testing"))
{
    // Testing-only code
}
```

---

# Configuration Files by Environment

ASP.NET automatically loads configuration files.

```text
appsettings.json
        ↓
appsettings.Development.json
        ↓
appsettings.Testing.json
        ↓
Environment variables
```

Later settings override earlier settings.

Example:

```text
appsettings.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=clinic-intake.db"
  }
}
```

Testing override:

```text
appsettings.Testing.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=clinic-intake-test.db"
  }
}
```

---

# Environment-Specific Behavior

Example:

```csharp
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}
```

Meaning:

```text
Development
    ↓
HTTPS enabled

Production
    ↓
HTTPS enabled

Testing
    ↓
HTTPS disabled
```

The application changes behavior automatically depending on where it runs.

---

# Integration Tests

Integration tests use a custom environment:

```csharp
public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
```

This forces tests to use:

```text
appsettings.Testing.json
```

instead of:

```text
appsettings.Development.json
```

---

# ASP.NET Environment Order

ASP.NET checks configuration in this order:

```text
Program.cs
        ↓
Environment
        ↓
Configuration files
        ↓
Services
        ↓
Middleware
```

The environment is determined before services are created.

Because of this, services can use different configuration depending on the environment.

---

# Typical Project Setup

```text
Development
    ↓
SQLite
    ↓
Swagger enabled
    ↓
Detailed logs
```

```text
Testing
    ↓
Separate test database
    ↓
Automated tests
    ↓
Reduced logs
```

```text
Production
    ↓
Azure SQL
    ↓
Swagger disabled
    ↓
Centralized logging
```

---

# Important Idea

Environments allow one application to run in multiple places without changing code.

```text
Environment
        ↓
Configuration
        ↓
Services
        ↓
Application behavior
```

A developer should change configuration, not source code.
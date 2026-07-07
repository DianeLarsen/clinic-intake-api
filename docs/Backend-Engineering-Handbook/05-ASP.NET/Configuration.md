# Configuration

## What Problem Does This Solve?

Applications need settings that change depending on where they run.

For example:

- Development vs Production
- Database connection strings
- API keys
- Logging options
- Authentication settings

Hard-coding these values into the application would require changing and recompiling the code every time a setting changes.

Configuration separates application settings from application logic.

## Solution

ASP.NET Core provides a built-in configuration system that loads settings from multiple sources.

Common configuration sources include:

- `appsettings.json`
- `appsettings.Development.json`
- Environment Variables
- Command-line arguments
- Azure Key Vault

The application reads configuration at startup and uses it to configure services.

## Why This Matters

Configuration allows the same application to run in different environments without changing the source code.

For example:

Development

```text
SQLite
```

Production

```text
Azure SQL Database
```

The code stays exactly the same.

Only the configuration changes.

## Mental Model

Think of configuration as the application's settings menu.

```text
Application
      │
      ▼
Configuration
      │
      ▼
Database
Logging
Authentication
API Keys
Feature Flags
```

The application asks configuration for values instead of storing them directly.

## Configuration Sources

ASP.NET Core combines settings from multiple places.

For example:

```text
appsettings.json
        ↓
appsettings.Development.json
        ↓
Environment Variables
        ↓
Command Line
```

Later sources override earlier ones.

This allows production servers to safely replace local settings.

## Real-World Example

In the Clinic Intake API, the database is configured like this:

```csharp
builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite(
        "Data Source=clinic-intake.db"));
```

The connection string is currently hard-coded.

A better approach is to store it in configuration.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection":
      "Data Source=clinic-intake.db"
  }
}
```

Then retrieve it:

```csharp
builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));
```

Now changing the database requires updating configuration rather than modifying code.

## Environment-Specific Configuration

ASP.NET Core supports multiple environments.

Examples:

```text
Development

Testing

Staging

Production
```

The current environment is available through:

```csharp
app.Environment
```

Example:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
}
```

Swagger is enabled only during development.

## Configuration vs Dependency Injection

These concepts are often confused.

Configuration answers:

```text
What values should the application use?
```

Dependency Injection answers:

```text
What objects should the application create?
```

Example:

Configuration provides:

```text
Connection String
```

Dependency Injection creates:

```text
ClinicIntakeDbContext
```

Configuration supplies the settings.

Dependency Injection supplies the objects.

## Common Configuration Examples

Connection Strings

```json
"ConnectionStrings":
{
    "DefaultConnection":
    "Data Source=clinic-intake.db"
}
```

---

Logging

```json
"Logging":
{
    "LogLevel":
    {
        "Default": "Information"
    }
}
```

---

Feature Flags

```json
"Features":
{
    "EnableRegistration": true
}
```

---

API Keys

Never store API keys directly in source code.

Use:

- Environment Variables
- Azure Key Vault
- Secret Manager (development)

## Common Beginner Questions

### Why not hard-code everything?

Because different environments need different settings.

Development may use SQLite.

Production may use Azure SQL.

Configuration allows both without changing the code.

---

### What is `builder.Configuration`?

It is ASP.NET Core's configuration system.

It combines settings from multiple sources into one object.

---

### Why are connection strings stored outside the code?

Connection strings often differ between environments and may contain sensitive information.

Keeping them in configuration makes the application easier to deploy and more secure.

---

### What is `appsettings.json`?

It is the default configuration file for an ASP.NET Core application.

It stores settings that the application reads during startup.

## Common Mistakes

- Hard-coding connection strings.
- Storing secrets in source control.
- Confusing configuration with Dependency Injection.
- Assuming configuration only comes from `appsettings.json`.
- Forgetting that production overrides development settings.

## Interview Answer

Configuration in ASP.NET Core provides application settings from sources such as JSON files, environment variables, and Azure Key Vault. It allows the same application to run in multiple environments without changing the code, making deployment more flexible and secure.

## One-Sentence Summary

Configuration separates application settings from application logic so the same code can run in different environments.

## What Finally Made It Click

- Configuration is not code.
- It is data that controls how the application behaves.
- The application asks configuration for values instead of storing them directly.
- Dependency Injection creates objects.
- Configuration supplies the values those objects need.
- The same application can connect to completely different databases simply by changing configuration.

## Related Topics

### Previous

- Program.cs
- Dependency Injection

### Next

- Middleware
- Azure Configuration

### See Also

- DbContext
- Azure SQL
- Key Vault
- App Service
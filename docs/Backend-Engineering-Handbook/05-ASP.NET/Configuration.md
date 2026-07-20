# Configuration in ASP.NET Core

## What Is Configuration?

Configuration is information the application needs but should not hard-code into its classes.

Examples include:

- Database connection strings
- Logging levels
- Pagination limits
- Authentication issuer and audience values
- URLs for external services
- Environment-specific behavior

Configuration lets the same compiled application run with different settings in Development, Testing, and Production.

## Real-World Analogy

Think of the application as a programmable medical device and configuration as its setup record. The program contains the rules for how the device operates. Configuration supplies values that may differ between installations, such as which server to contact or which limits to use. Changing a setting should not require taking the program apart and soldering a new number into the code.

## `builder.Configuration`

This line creates the application builder:

```csharp
var builder = WebApplication.CreateBuilder(args);
```

ASP.NET automatically loads configuration from several sources and combines them into:

```csharp
builder.Configuration
```

The application sees one combined collection of configuration keys and values, even though those values came from several places.

## Default Configuration Sources

The usual sources, shown from lower to higher priority, are:

```text
appsettings.json
    ↓ overridden by
appsettings.{Environment}.json
    ↓ overridden by
User secrets in Development
    ↓ overridden by
Environment variables
    ↓ overridden by
Command-line arguments
```

When two sources provide the same key, the higher-priority source wins.

Example:

```text
appsettings.json:
Pagination:DefaultPageSize = 10

Environment variable:
Pagination__DefaultPageSize = 3

Effective value:
Pagination:DefaultPageSize = 3
```

## Base Settings

`appsettings.json` contains settings shared by environments:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=clinic-intake.db"
  },
  "Database": {
    "Provider": "Sqlite"
  },
  "Pagination": {
    "DefaultPageSize": 10,
    "MaximumPageSize": 100
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

`appsettings.json` should contain safe defaults, not passwords or production secrets.

## Environment-Specific Overrides

ASP.NET loads a second JSON file matching the current environment.

In Development, it loads:

```text
appsettings.Development.json
```

For this project, that file contains local JWT information:

```json
{
  "Authentication": {
    "Schemes": {
      "Bearer": {
        "ValidAudiences": ["http://localhost:5090"],
        "ValidIssuer": "dotnet-user-jwts"
      }
    }
  }
}
```

It does not repeat connection or logging settings because it inherits them from `appsettings.json`.

## Hierarchical Keys

JSON settings form a hierarchy:

```json
{
  "Pagination": {
    "DefaultPageSize": 10
  }
}
```

ASP.NET represents the full key with colons:

```text
Pagination:DefaultPageSize
```

For environment variables, use double underscores because colons do not work consistently on every platform:

```text
Pagination__DefaultPageSize
```

ASP.NET converts the double underscore into a colon.

## Reading a Connection String

`Program.cs` reads the database connection string like this:

```csharp
string connectionString =
    builder.Configuration
        .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found."
    );
```

`GetConnectionString("DefaultConnection")` is a helper that reads:

```text
ConnectionStrings:DefaultConnection
```

This code fails immediately if the required value is missing. That is better than starting successfully and failing during the first database request.

## Selecting a Database Provider

The Clinic Intake API reads the database provider from configuration:

```csharp
string databaseProvider =
    builder.Configuration["Database:Provider"] ?? "Sqlite";
```

Supported values are:

| Provider value | Entity Framework provider | Intended use                                     |
| -------------- | ------------------------- | ------------------------------------------------ |
| `Sqlite`       | `UseSqlite()`             | Local development and isolated integration tests |
| `SqlServer`    | `UseSqlServer()`          | Azure SQL or another SQL Server database         |

The default is `Sqlite`, so local development continues to use:

```text
Data Source=clinic-intake.db
```

An Azure deployment will eventually use environment settings like:

```text
Database__Provider = SqlServer
ConnectionStrings__DefaultConnection = <Azure SQL connection string>
```

The connection string is a secret and must be configured in Azure, not committed to `appsettings.json`.

### Provider-Specific Migrations

SQLite and SQL Server use different SQL dialects and column definitions.

The project's existing migrations were generated for SQLite. Setting `Database__Provider` to `SqlServer` is not enough by itself to deploy to Azure SQL; the project must also add SQL Server-compatible migrations.

This configuration change creates the provider-selection seam first. SQL Server migration support is the next Azure-readiness step.

## Environment-Variable Overrides

This command temporarily changes the default page size for one process:

```bash
Pagination__DefaultPageSize=3 \
  dotnet run --launch-profile http
```

It does not modify a JSON file. When the process stops, that temporary override disappears.

Azure App Settings use the same configuration-key format. An Azure setting named:

```text
Pagination__DefaultPageSize
```
Database__Provider = SqlServer

overrides the JSON value without rebuilding the application.

## Configuration Versus Secrets

Configuration is not automatically secret.

Safe examples include:

- Pagination limits
- Log levels
- Public URLs
- Issuer names
- Audience names

Secret examples include:

- Passwords
- Database passwords
- API keys
- JWT signing keys
- OAuth client secrets

Do not put secrets in committed JSON files. Use .NET user secrets locally and a controlled production secret store such as Azure Key Vault.

## Strongly Typed Configuration

For related values, prefer the Options pattern instead of asking classes to retrieve individual strings from `IConfiguration`.

```text
JSON section
    ↓ bound into
C# options object
    ↓ injected into
Controller or service
```

For example:

```json
"Pagination": {
  "DefaultPageSize": 10,
  "MaximumPageSize": 100
}
```

becomes a `PaginationOptions` object with integer properties.

## Fail-Fast Configuration

Required configuration should be validated during startup.

This project validates that:

```text
DefaultPageSize > 0
MaximumPageSize >= DefaultPageSize
```

An invalid environment-variable override such as:

```bash
Pagination__MaximumPageSize=5 \
  dotnet run --launch-profile http
```

fails because the configured default is 10. Refusing to start prevents the application from accepting requests with impossible rules.

## Common Mistakes

### Repeating identical settings

Do not copy the same value into every environment file. Put shared values in `appsettings.json` and only genuine overrides in environment-specific files.

### Treating JSON as a secret store

An ignored file can still be copied, logged, backed up, or accidentally committed. Use a secrets system for secrets.

### Hard-coding operational values

This is configuration hidden inside source code:

```csharp
int pageSize = 10;
```

If the number is an operational rule that may vary, bind it from configuration.

### Logging every configuration value

Dumping all configuration for debugging can expose passwords, tokens, and signing keys. Log only specifically approved non-secret values.

## Key Points

```text
Configuration
    → Values the application needs to operate

builder.Configuration
    → Combined view of all configuration sources

Later provider
    → Overrides an earlier provider

Double underscore
    → Represents hierarchy in environment variables

Fail fast
    → Reject invalid settings during startup
```

## Reference

- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)


The project now contains a SQL Server baseline migration named `InitialSqlServer`. Azure SQL uses that migration history through `MigrateAsync()`, while disposable SQLite development and test databases use `EnsureCreatedAsync()`.
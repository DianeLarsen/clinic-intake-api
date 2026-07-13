# Configuration Cheat Sheet

## Configuration File Order

ASP.NET loads configuration in this order:

```text
appsettings.json
        ↓
appsettings.Development.json
        ↓
appsettings.Testing.json
        ↓
Environment variables
        ↓
Command-line arguments
````

Later values override earlier values.

---

# Create a Connection String

## appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=clinic-intake.db"
  }
}
```

---

## appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=clinic-intake.db"
  }
}
```

---

## appsettings.Testing.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=clinic-intake-test.db"
  }
}
```

---

# Read Configuration in Program.cs

```csharp
string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string was not found."
    );
```

---

# Register DbContext

```csharp
builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite(connectionString)
);
```

---

# Check the Current Environment

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
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

# Set the Environment

## Development

```bash
export ASPNETCORE_ENVIRONMENT=Development
```

---

## Testing

```bash
export ASPNETCORE_ENVIRONMENT=Testing
```

---

## Production

```bash
export ASPNETCORE_ENVIRONMENT=Production
```

---

# Integration Test Environment

Set the environment inside:

```text
ClinicIntakeApi.Tests/
    Integration/
        CustomWebApplicationFactory.cs
```

```csharp
protected override void ConfigureWebHost(
    IWebHostBuilder builder)
{
    builder.UseEnvironment("Testing");
}
```

---

# Common Database Setup

```text
Development
    ↓
clinic-intake.db

Testing
    ↓
clinic-intake-test.db

Production
    ↓
Azure SQL
```

---

# Common Pattern

Step 1:

Create the setting:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  }
}
```

↓

Step 2:

Read the setting:

```csharp
builder.Configuration.GetConnectionString(
    "DefaultConnection"
);
```

↓

Step 3:

Use the setting:

```csharp
options.UseSqlite(connectionString);
```

---

# Common Mistakes

❌ Hard-coding values:

```csharp
options.UseSqlite("Data Source=clinic-intake.db");
```

---

❌ Putting secrets in source code:

```csharp
string apiKey = "abc123";
```

---

❌ Using the development database in tests.

---

❌ Forgetting to create:

```text
appsettings.Testing.json
```

---

# Mental Model

```text
JSON file
    ↓
builder.Configuration
    ↓
Program.cs
    ↓
DbContext
    ↓
Database
```



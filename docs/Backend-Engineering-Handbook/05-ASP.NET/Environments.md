# Environments in ASP.NET Core

## What Is an Environment?

An environment tells the application where it is running.

Common environment names are:

```text
Development
Testing
Staging
Production
```

The application code can remain the same while configuration and selected behavior change for each environment.

## Real-World Analogy

Think of environments as separate work areas for a medical device. Development is the engineering bench, where diagnostic tools are visible. Testing is the verification station, where predictable test equipment replaces real dependencies. Production is the clinical setting, where diagnostic shortcuts are removed and security rules are strict. It is the same design, but the surrounding setup is different.

## Development

Development is used while building and debugging locally.

This project uses Development to:

- Load `appsettings.Development.json`
- Load .NET user secrets
- Enable Swagger
- Accept tokens created by `dotnet user-jwts`
- Run on local ports such as `5090`

Development may include helpful debugging behavior that should not be exposed publicly.

## Testing

Testing is used by the integration-test server.

`CustomWebApplicationFactory` selects it:

```csharp
builder.UseEnvironment("Testing");
```

In Testing:

- HTTPS redirection is skipped
- A predictable test authentication handler replaces JWT authentication
- Integration tests call the API in memory

The test-only authentication handler does not replace JWT authentication in the normal application.

## Production

Production is the deployed application used by real clients.

Production should:

- Avoid detailed development error pages
- Avoid local test authentication
- Receive secrets from deployment configuration
- Use production database and identity services
- Use appropriate logging and monitoring
- Disable development-only Swagger unless there is a secured reason to expose it

If no environment is set, ASP.NET Core defaults to Production.

## Selecting the Local Environment

The local launch profile contains:

```json
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development"
}
```

This tells the application to use Development when launched with that profile.

Run a specific profile with:

```bash
dotnet run --launch-profile http
```

The `http` profile also provides:

```json
"applicationUrl": "http://localhost:5090"
```

## `launchSettings.json` Is Local Only

`Properties/launchSettings.json` controls local development launches.

It can define:

- Local URLs and ports
- Whether to open a browser
- Which page to open
- Local environment variables
- Multiple launch profiles

It is not published with the application and does not configure Azure. Azure uses its own application settings.

## Environment-Specific Files

ASP.NET loads the base file first:

```text
appsettings.json
```

Then it loads the file matching the environment:

```text
appsettings.Development.json
appsettings.Testing.json
appsettings.Staging.json
appsettings.Production.json
```

The environment file overrides matching base keys.

For Development:

```text
appsettings.json
    ↓ overridden by
appsettings.Development.json
```

Do not create an environment file merely to duplicate every base setting. Include only deliberate differences.

## Checking the Current Environment

After building the application, ASP.NET exposes the current environment through:

```csharp
app.Environment
```

Examples:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

```csharp
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}
```

The first example enables Swagger only in Development. The second avoids HTTPS redirection inside the in-memory test server.

## Setting an Environment from the Command Line

One supported approach is:

```bash
dotnet run --environment Staging
```

When using a shell environment variable directly, run without a launch profile so the launch profile does not replace the selected environment:

```bash
ASPNETCORE_ENVIRONMENT=Staging \
  dotnet run --no-launch-profile
```

## Environment Is Not Authorization

An environment name is application setup, not user permission.

```text
Development / Production
    → Where the application runs

User / Admin
    → What an authenticated user may do
```

Do not use an environment name as a substitute for authentication or authorization.

## `anonymousAuthentication` in IIS Settings

The local file may contain:

```json
"anonymousAuthentication": true
```

This lets IIS Express pass requests into ASP.NET. It does not bypass `[Authorize]` or JWT validation inside the application.

The layers are:

```text
IIS Express accepts request
    ↓
ASP.NET authentication checks token
    ↓
ASP.NET authorization checks access rules
```

## Azure Environments

Azure App Service uses Production by default unless configured otherwise.

Azure App Settings can provide values such as:

```text
ASPNETCORE_ENVIRONMENT = Production
Pagination__DefaultPageSize = 10
ConnectionStrings__DefaultConnection = <production value>
```

Changing an Azure App Setting restarts the application so it reloads configuration.

## Common Mistakes

### Deploying Development behavior

Do not expose development diagnostics, fake authentication, or local token issuers in Production.

### Assuming launch settings configure Azure

Local launch profiles are not deployed. Production ports, settings, and environment values must be configured on the hosting platform.

### Putting secrets in an environment JSON file

`appsettings.Production.json` is still a source-controlled file unless deliberately excluded. A filename containing `Production` does not turn it into a vault.

### Changing behavior unnecessarily

Environments should mainly change configuration and infrastructure choices. Large differences in application logic make Testing less representative of Production.

## Key Points

```text
Development
    → Local building and debugging

Testing
    → Predictable automated verification

Production
    → Deployed real application

launchSettings.json
    → Local launch behavior only

ASPNETCORE_ENVIRONMENT
    → Selects the current environment
```

## Reference

- [ASP.NET Core runtime environments](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments)

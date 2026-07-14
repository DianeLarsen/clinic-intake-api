# The Options Pattern in ASP.NET Core

## What Is the Options Pattern?

The Options pattern converts a related configuration section into a normal C# object.

Instead of repeatedly requesting individual strings such as:

```csharp
configuration["Pagination:DefaultPageSize"]
```

the application receives:

```csharp
PaginationOptions
```

with typed integer properties.

## Real-World Analogy

Think of raw configuration as parts stored in labeled bins. A class could walk through the warehouse and retrieve each part individually, but that spreads storage knowledge throughout the application. The Options pattern has the configuration system assemble the parts into a complete pagination kit and deliver that typed kit to the controller through dependency injection.

## The Configuration Section

`appsettings.json` contains:

```json
"Pagination": {
  "DefaultPageSize": 10,
  "MaximumPageSize": 100
}
```

This section contains two related pagination rules.

## The Options Class

Create `Configuration/PaginationOptions.cs`:

```csharp
namespace ClinicIntakeApi.Configuration;

/// <summary>
/// Represents pagination settings loaded from configuration.
/// </summary>
public class PaginationOptions
{
    // Must match the JSON section name.
    public const string SectionName = "Pagination";

    // Used when the client omits pageSize.
    public int DefaultPageSize { get; set; }

    // Largest page size the client may request.
    public int MaximumPageSize { get; set; }
}
```

The property names match the JSON names:

| JSON | C# property |
| --- | --- |
| `DefaultPageSize` | `DefaultPageSize` |
| `MaximumPageSize` | `MaximumPageSize` |

The section-name constant prevents the text `"Pagination"` from being repeated across the application.

## Bind and Register the Options

Register the options in `Program.cs`:

```csharp
builder.Services
    .AddOptions<PaginationOptions>()
    .Bind(
        builder.Configuration.GetSection(
            PaginationOptions.SectionName
        )
    )
    .Validate(
        options => options.DefaultPageSize > 0,
        "Pagination:DefaultPageSize must be greater than 0."
    )
    .Validate(
        options =>
            options.MaximumPageSize
            >= options.DefaultPageSize,
        "Pagination:MaximumPageSize must be greater than or equal to DefaultPageSize."
    )
    .ValidateOnStart();
```

This performs four jobs:

```text
AddOptions
    → Register the options type

Bind
    → Copy JSON values into the object

Validate
    → Check the business rules

ValidateOnStart
    → Refuse to start when invalid
```

## Why Validate During Startup?

Without startup validation, invalid settings may remain hidden until an endpoint tries to use them.

For example:

```text
DefaultPageSize = 10
MaximumPageSize = 5
```

This configuration is impossible because the default is already larger than the allowed maximum.

`ValidateOnStart()` produces a clear startup failure instead of allowing unpredictable request behavior.

## Inject the Options

Import:

```csharp
using ClinicIntakeApi.Configuration;
using Microsoft.Extensions.Options;
```

Add a field to `RequestsController`:

```csharp
private readonly PaginationOptions _paginationOptions;
```

Inject the wrapper through the constructor:

```csharp
public RequestsController(
    IIntakeService intakeService,
    IOptions<PaginationOptions> paginationOptions
)
{
    _intakeService = intakeService;
    _paginationOptions = paginationOptions.Value;
}
```

`IOptions<PaginationOptions>` is the wrapper supplied by dependency injection. `.Value` retrieves the actual configured object.

```text
IOptions<PaginationOptions>
    → Configuration envelope

.Value
    → PaginationOptions inside the envelope
```

## Use the Configured Default

The query parameter becomes nullable:

```csharp
int? pageSize = null
```

If the client omits it, use the configured default:

```csharp
int resolvedPageSize =
    pageSize ?? _paginationOptions.DefaultPageSize;
```

Then enforce the configured maximum:

```csharp
if (
    resolvedPageSize < 1
    || resolvedPageSize
        > _paginationOptions.MaximumPageSize
)
{
    return BadRequest(
        $"PageSize must be between 1 and {_paginationOptions.MaximumPageSize}."
    );
}
```

The flow becomes:

```text
Client supplies pageSize
    → Validate and use it

Client omits pageSize
    → Use configured default

Client exceeds maximum
    → Return 400 Bad Request
```

## `IOptions<T>`

```csharp
IOptions<PaginationOptions>
```

Use it when the application can keep one options value until restart.

This project uses it because pagination limits are stable application rules.

## `IOptionsSnapshot<T>`

```csharp
IOptionsSnapshot<PaginationOptions>
```

It is a scoped service that creates a snapshot when it is constructed. In a web application, that usually means a fresh snapshot for each request scope.

Use it when scoped or transient components should see configuration changes on later requests.

Do not inject it into singleton services because a scoped service cannot safely be captured by a singleton.

## `IOptionsMonitor<T>`

```csharp
IOptionsMonitor<PaginationOptions>
```

It is a singleton service that can retrieve the current value and react to changes:

```csharp
optionsMonitor.OnChange(updatedOptions =>
{
    // React to the new values.
});
```

Use it for singleton or long-running components that truly must respond to configuration changes without restarting.

## Which One Should Be Used Here?

Keep:

```csharp
IOptions<PaginationOptions>
```

Pagination limits should not unexpectedly change halfway through the process. Restarting after changing them is simple and predictable.

## Integration Tests

The default-setting test proves the complete binding path:

```csharp
[Fact]
public async Task GetRequests_WhenPageSizeIsMissing_UsesConfiguredDefault()
{
    HttpResponseMessage response =
        await _client.GetAsync("/api/v1/requests");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    PagedResponse<RequestSummaryDto>? responseBody =
        await response.Content
            .ReadFromJsonAsync<
                PagedResponse<RequestSummaryDto>
            >();

    Assert.NotNull(responseBody);
    Assert.Equal(10, responseBody.PageSize);
}
```

The maximum-setting test proves the configured limit is enforced:

```csharp
[Fact]
public async Task GetRequests_WhenPageSizeExceedsMaximum_ReturnsBadRequest()
{
    HttpResponseMessage response =
        await _client.GetAsync(
            "/api/v1/requests?pageSize=101"
        );

    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode
    );
}
```

## Why Not Inject `IConfiguration` Everywhere?

This works:

```csharp
configuration.GetValue<int>(
    "Pagination:DefaultPageSize"
);
```

But spreading it throughout controllers and services causes problems:

- Key names are repeated as strings
- Values can be requested using the wrong type
- Related settings are scattered
- Validation becomes inconsistent
- Classes become tied to configuration storage details

The Options pattern provides one typed, validated object instead.

## Key Points

```text
Options class
    → C# shape of one configuration section

Bind
    → Copies configuration into the object

Validate
    → Checks configuration rules

ValidateOnStart
    → Fails before accepting requests

IOptions<T>.Value
    → Retrieves the configured object
```

## Reference

- [Options pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)

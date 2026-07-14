using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClinicIntakeApi.Tests.Integration;

public class HealthCheckApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public HealthCheckApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetLiveness_WithoutToken_ReturnsHealthy()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health/live");

        string responseBody = await response.Content.ReadAsStringAsync();

        using JsonDocument json = JsonDocument.Parse(responseBody);

        JsonElement root = json.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal("Healthy", root.GetProperty("status").GetString());

        // Liveness deliberately runs no dependency checks.
        Assert.Equal(0, root.GetProperty("checks").GetArrayLength());
    }

    [Fact]
    public async Task GetReadiness_WhenDatabaseIsAvailable_ReturnsHealthy()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health/ready");

        string responseBody = await response.Content.ReadAsStringAsync();

        using JsonDocument json = JsonDocument.Parse(responseBody);

        JsonElement root = json.RootElement;
        JsonElement checks = root.GetProperty("checks");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal("Healthy", root.GetProperty("status").GetString());

        // Readiness currently contains one dependency check.
        Assert.Equal(1, checks.GetArrayLength());

        JsonElement databaseCheck = checks[0];

        Assert.Equal("database", databaseCheck.GetProperty("name").GetString());

        Assert.Equal("Healthy", databaseCheck.GetProperty("status").GetString());
    }

    [Fact]
    public async Task GetReadiness_WhenARequiredCheckFails_ReturnsServiceUnavailable()
    {
        // Arrange
        //
        // Create a special test server based on the normal factory.
        // Add one deliberately failing check tagged "ready".
        using var unhealthyFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddCheck(
                        "forced-failure",
                        () => HealthCheckResult.Unhealthy("Forced failure for testing."),
                        tags: ["ready"]
                    );
            });
        });

        using HttpClient client = unhealthyFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health/ready");

        string responseBody = await response.Content.ReadAsStringAsync();

        using JsonDocument json = JsonDocument.Parse(responseBody);

        JsonElement root = json.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        Assert.Equal("Unhealthy", root.GetProperty("status").GetString());
    }
}

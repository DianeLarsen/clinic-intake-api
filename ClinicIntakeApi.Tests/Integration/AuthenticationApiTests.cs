using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ClinicIntakeApi.Tests.Integration;

public class AuthenticationApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    // The factory creates a test version of the complete API.
    private readonly WebApplicationFactory<Program> _factory;

    public AuthenticationApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRequests_WhenTokenIsMissing_ReturnsUnauthorized()
    {
        // Arrange
        //
        // Create a client without an Authorization header.
        using HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/v1/requests");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRequests_WhenTokenIsInvalid_ReturnsUnauthorized()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // Send a Bearer token, but use the wrong token value.
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "wrong-token"
        );

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/v1/requests");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRequests_WhenTokenIsValid_ReturnsOk()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // Send the valid demo token.
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "demo-token"
        );

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/v1/requests");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

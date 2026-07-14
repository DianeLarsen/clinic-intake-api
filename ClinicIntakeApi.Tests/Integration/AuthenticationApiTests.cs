using System.Net;
using System.Net.Http.Headers;

namespace ClinicIntakeApi.Tests.Integration;

public class AuthenticationApiTests : IClassFixture<CustomWebApplicationFactory>
{
    // The factory creates a test version of the complete API.
    private readonly CustomWebApplicationFactory _factory;

    public AuthenticationApiTests(CustomWebApplicationFactory factory)
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

    [Fact]
    public async Task GetRequests_WhenUserIsAuthenticated_ReturnsOk()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // Use the ordinary User token.
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "demo-user-token"
        );

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/v1/requests");

        // Assert
        //
        // The controller requires authentication,
        // but this endpoint does not require the Admin role.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRequest_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // This is a valid token, but it has the User role.
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "demo-user-token"
        );

        // Act
        HttpResponseMessage response = await client.DeleteAsync("/api/v1/requests/999999");

        // Assert
        //
        // The user is authenticated, but does not have
        // the Admin role required by the delete endpoint.
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRequest_WhenUserIsAdmin_ReachesController()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // The original demo token has the Admin role.
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "demo-token"
        );

        // Act
        HttpResponseMessage response = await client.DeleteAsync("/api/v1/requests/999999");

        // Assert
        //
        // NotFound proves authorization allowed the request
        // to enter the controller. The controller then looked
        // for request 999999 and could not find it.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

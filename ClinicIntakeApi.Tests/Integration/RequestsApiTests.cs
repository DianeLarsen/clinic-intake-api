using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClinicIntakeApi.Data;
using ClinicIntakeApi.Dtos;
using ClinicIntakeApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicIntakeApi.Tests.Integration;

public class RequestsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RequestsApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        // Give every request from this test client the valid demo token.
        // These tests are testing controller behavior, not failed authentication.
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "demo-token"
        );
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// Creates an authenticated test client belonging
    /// to the specified clinic.
    /// </summary>
    private HttpClient CreateClientForClinic(int clinicId)
    {
        HttpClient client = _factory.CreateClient();

        // Use the valid Admin test token.
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "demo-token"
        );

        // Override the fake user's ClinicId claim.
        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.ClinicIdHeaderName,
            clinicId.ToString()
        );

        return client;
    }

    [Fact]
    public async Task GetRequests_ReturnsOk()
    {
        // Act

        HttpResponseMessage response = await _client.GetAsync("/api/v1/requests");

        // Assert

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(
            "application/json; charset=utf-8",
            response.Content.Headers.ContentType?.ToString()
        );
    }

    [Fact]
    public async Task CreateRequest_WhenPatientExists_ReturnsCreated()
    {
        // Arrange

        int patientId;
        int clinicId;

        // Open the database only long enough to read a patient.
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            ClinicIntakeDbContext db =
                scope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

            Patient patient = await db
                .Patients.AsNoTracking()
                .OrderBy(patient => patient.Id)
                .FirstAsync();

            patientId = patient.Id;
            clinicId = patient.ClinicId;
        }

        // The database scope is now closed.

        var dto = new CreateRequestDto { PatientId = patientId };

        // Act

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/requests", dto);

        // Read the body before asserting.
        // This helps us see what the API returned if the test fails.
        string responseBody = await response.Content.ReadAsStringAsync();

        // Assert

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Expected 201 Created, but received "
                + $"{(int)response.StatusCode} {response.StatusCode}. "
                + $"Response body: {responseBody}"
        );

        IntakeRequestResponseDto? createdRequest =
            await response.Content.ReadFromJsonAsync<IntakeRequestResponseDto>(JsonOptions);

        Assert.NotNull(createdRequest);
        Assert.Equal(patientId, createdRequest.PatientId);
        Assert.Equal(clinicId, createdRequest.ClinicId);

        HttpResponseMessage deleteResponse = await _client.DeleteAsync(
            $"/api/v1/requests/{createdRequest.Id}"
        );

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task CreateRequest_WhenPatientIdIsNegative_ReturnsValidationError()
    {
        // Arrange

        var dto = new CreateRequestDto { PatientId = -1 };

        // Act

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/requests", dto);

        string responseBody = await response.Content.ReadAsStringAsync();

        // Assert

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        Assert.Contains("PatientId must be greater than 0.", responseBody);
    }

    [Fact]
    public async Task CreateRequest_WhenPatientDoesNotExist_ReturnsBadRequest()
    {
        // Arrange

        var dto = new CreateRequestDto { PatientId = 999999 };

        // Act

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/requests", dto);

        string responseBody = await response.Content.ReadAsStringAsync();

        // Assert

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        Assert.Contains("does not exist", responseBody);
    }

    [Fact]
    public async Task UpdateStatus_WhenRequestExists_UpdatesStatus()
    {
        // Arrange

        int requestId;

        //
        // Open a temporary database scope.
        //
        // We only use this scope to find a request ID.
        //
        using (IServiceScope arrangeScope = _factory.Services.CreateScope())
        {
            ClinicIntakeDbContext db =
                arrangeScope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

            requestId = await db
                .IntakeRequests.AsNoTracking()
                .OrderBy(request => request.Id)
                .Select(request => request.Id)
                .FirstAsync();
        }

        //
        // The first database scope is now closed.
        //
        // This prevents the test from holding an old copy
        // of the request in memory.
        //

        var dto = new UpdateRequestStatusDto { Status = RequestStatus.Completed };

        // Act

        //
        // Send a real HTTP PUT request to the API.
        //
        HttpResponseMessage updateResponse = await _client.PutAsJsonAsync(
            $"/api/v1/requests/{requestId}/status",
            dto,
            JsonOptions
        );

        // Assert

        //
        // The API should report that the update succeeded.
        //
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        //
        // Open a brand-new database scope.
        //
        // This reads the current value directly from SQLite
        // instead of using an older object already held in memory.
        //
        using IServiceScope assertScope = _factory.Services.CreateScope();

        ClinicIntakeDbContext assertDb =
            assertScope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

        RequestStatus savedStatus = await assertDb
            .IntakeRequests.AsNoTracking()
            .Where(request => request.Id == requestId)
            .Select(request => request.Status)
            .SingleAsync();

        //
        // Verify that SQLite now stores Completed.
        //
        Assert.Equal(RequestStatus.Completed, savedStatus);
    }

    [Fact]
    public async Task DeleteRequest_WhenRequestExists_RemovesRequest()
    {
        // Arrange

        int patientId;

        // Open the database only long enough to read a valid patient ID.
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            ClinicIntakeDbContext db =
                scope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

            patientId = await db
                .Patients.AsNoTracking()
                .OrderBy(patient => patient.Id)
                .Select(patient => patient.Id)
                .FirstAsync();
        }

        // Create a request that belongs only to this test.
        var dto = new CreateRequestDto { PatientId = patientId };

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/requests", dto);

        string createResponseBody = await createResponse.Content.ReadAsStringAsync();

        Assert.True(
            createResponse.StatusCode == HttpStatusCode.Created,
            $"Expected 201 Created, but received "
                + $"{(int)createResponse.StatusCode} {createResponse.StatusCode}. "
                + $"Response body: {createResponseBody}"
        );

        IntakeRequestResponseDto? createdRequest =
            await createResponse.Content.ReadFromJsonAsync<IntakeRequestResponseDto>(JsonOptions);

        Assert.NotNull(createdRequest);

        // Act

        // Delete the exact request this test created.
        HttpResponseMessage deleteResponse = await _client.DeleteAsync(
            $"/api/v1/requests/{createdRequest.Id}"
        );

        // Assert

        // The API should report a successful delete.
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Open a new database scope so we read fresh data.
        using IServiceScope assertScope = _factory.Services.CreateScope();

        ClinicIntakeDbContext assertDb =
            assertScope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

        bool requestStillExists = await assertDb
            .IntakeRequests.AsNoTracking()
            .AnyAsync(request => request.Id == createdRequest.Id);

        // The request should no longer exist in SQLite.
        Assert.False(requestStillExists);
    }

    [Fact]
    public async Task DeleteRequest_WhenRequestDoesNotExist_ReturnsNotFound()
    {
        // Arrange

        // Use an ID that should not exist in the database.
        int nonExistentRequestId = -1;

        // Act

        HttpResponseMessage deleteResponse = await _client.DeleteAsync(
            $"/api/v1/requests/{nonExistentRequestId}"
        );

        // Assert

        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task CreateRequest_WhenPatientIdIsNegative_ReturnsBadRequest()
    {
        // Arrange

        var dto = new CreateRequestDto { PatientId = -5 };

        // Act

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/requests", dto);

        // Assert

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_WhenStatusIsInvalid_ReturnsBadRequest()
    {
        // Arrange

        int requestId;

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            ClinicIntakeDbContext db =
                scope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

            requestId = await db
                .IntakeRequests.AsNoTracking()
                .OrderBy(request => request.Id)
                .Select(request => request.Id)
                .FirstAsync();
        }

        //
        // 999 is not a valid RequestStatus value.
        //
        var dto = new UpdateRequestStatusDto { Status = (RequestStatus)999 };

        // Act

        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/v1/requests/{requestId}/status",
            dto,
            JsonOptions
        );

        string responseBody = await response.Content.ReadAsStringAsync();

        // Assert

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        Assert.Contains("Status", responseBody);
    }

    [Fact]
    public async Task GetRequests_WhenPageSizeIsMissing_UsesConfiguredDefault()
    {
        // Arrange
        //
        // Do not include pageSize in the query string.
        // appsettings.json says the default should be 10.

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/requests");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PagedResponse<RequestSummaryDto>? responseBody = await response.Content.ReadFromJsonAsync<
            PagedResponse<RequestSummaryDto>
        >();

        Assert.NotNull(responseBody);

        // Proves the controller used Pagination:DefaultPageSize.
        Assert.Equal(10, responseBody.PageSize);
    }

    [Fact]
    public async Task GetRequests_WhenPageSizeExceedsMaximum_ReturnsBadRequest()
    {
        // Arrange
        //
        // appsettings.json sets MaximumPageSize to 100.
        // Requesting 101 should be rejected.

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/requests?pageSize=101");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        string responseBody = await response.Content.ReadAsStringAsync();

        Assert.Contains("PageSize must be between 1 and 100", responseBody);
    }

    [Fact]
    public async Task GetRequests_WhenPageIsLessThanOne_ReturnsBadRequest()
    {
        // Arrange
        //
        // Page numbering begins at 1.

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/requests?page=0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        string responseBody = await response.Content.ReadAsStringAsync();

        Assert.Contains("Page must be greater than or equal to 1", responseBody);
    }

    [Fact]
    public async Task GetRequestById_WhenRequestBelongsToAnotherClinic_ReturnsNotFound()
    {
        // Arrange
        int requestId;
        int ownerClinicId;
        int otherClinicId;

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            ClinicIntakeDbContext db =
                scope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

            // Select one real request from the seeded database.
            IntakeRequest targetRequest = await db
                .IntakeRequests.AsNoTracking()
                .OrderBy(request => request.Id)
                .FirstAsync();

            requestId = targetRequest.Id;
            ownerClinicId = targetRequest.ClinicId;

            // Select a different clinic.
            otherClinicId = await db
                .Clinics.AsNoTracking()
                .Where(clinic => clinic.Id != ownerClinicId)
                .OrderBy(clinic => clinic.Id)
                .Select(clinic => clinic.Id)
                .FirstAsync();
        }

        using HttpClient ownerClient = CreateClientForClinic(ownerClinicId);

        using HttpClient otherClinicClient = CreateClientForClinic(otherClinicId);

        // Act
        HttpResponseMessage ownerResponse = await ownerClient.GetAsync(
            $"/api/v1/requests/{requestId}"
        );

        HttpResponseMessage otherClinicResponse = await otherClinicClient.GetAsync(
            $"/api/v1/requests/{requestId}"
        );

        // Assert

        // The owning clinic can retrieve the request.
        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);

        // Another clinic sees the same result it would receive
        // for a nonexistent request.
        Assert.Equal(HttpStatusCode.NotFound, otherClinicResponse.StatusCode);
    }

    [Fact]
    public async Task GetRequests_ReturnsOnlyRequestsFromAuthenticatedClinic()
    {
        // Arrange
        int clinicId;
        List<int> expectedRequestIds;
        List<int> foreignRequestIds;

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            ClinicIntakeDbContext db =
                scope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

            // Select a clinic that owns at least one request.
            clinicId = await db
                .IntakeRequests.AsNoTracking()
                .OrderBy(request => request.Id)
                .Select(request => request.ClinicId)
                .FirstAsync();

            // Record every request this clinic should receive.
            expectedRequestIds = await db
                .IntakeRequests.AsNoTracking()
                .Where(request => request.ClinicId == clinicId)
                .Select(request => request.Id)
                .OrderBy(id => id)
                .ToListAsync();

            // Record requests belonging to other clinics.
            foreignRequestIds = await db
                .IntakeRequests.AsNoTracking()
                .Where(request => request.ClinicId != clinicId)
                .Select(request => request.Id)
                .ToListAsync();
        }

        using HttpClient clinicClient = CreateClientForClinic(clinicId);

        // Act
        //
        // Use the maximum configured page size so all current
        // seeded requests for this clinic fit on one page.
        HttpResponseMessage response = await clinicClient.GetAsync("/api/v1/requests?pageSize=100");

        PagedResponse<RequestSummaryDto>? responseBody = await response.Content.ReadFromJsonAsync<
            PagedResponse<RequestSummaryDto>
        >(JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotNull(responseBody);

        List<int> actualRequestIds = responseBody
            .Items.Select(item => item.Id)
            .OrderBy(id => id)
            .ToList();

        // The clinic receives exactly its own request IDs.
        Assert.Equal(expectedRequestIds, actualRequestIds);

        // No foreign request ID appears in the response.
        Assert.DoesNotContain(actualRequestIds, id => foreignRequestIds.Contains(id));
    }

    [Fact]
    public async Task CreateRequest_WhenPatientBelongsToAnotherClinic_ReturnsBadRequest()
    {
        // Arrange
        int patientId;
        int patientClinicId;
        int otherClinicId;
        int requestCountBefore;

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            ClinicIntakeDbContext db =
                scope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

            Patient patient = await db
                .Patients.AsNoTracking()
                .OrderBy(patient => patient.Id)
                .FirstAsync();

            patientId = patient.Id;
            patientClinicId = patient.ClinicId;

            // Choose a clinic that does not own this patient.
            otherClinicId = await db
                .Clinics.AsNoTracking()
                .Where(clinic => clinic.Id != patientClinicId)
                .OrderBy(clinic => clinic.Id)
                .Select(clinic => clinic.Id)
                .FirstAsync();

            // Record how many requests already exist
            // for this patient.
            requestCountBefore = await db.IntakeRequests.CountAsync(request =>
                request.PatientId == patientId
            );
        }

        using HttpClient otherClinicClient = CreateClientForClinic(otherClinicId);

        var dto = new CreateRequestDto { PatientId = patientId };

        // Act
        HttpResponseMessage response = await otherClinicClient.PostAsJsonAsync(
            "/api/v1/requests",
            dto
        );

        string responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        Assert.Contains("does not exist", responseBody);

        // Confirm that the failed request created no record.
        using (IServiceScope assertScope = _factory.Services.CreateScope())
        {
            ClinicIntakeDbContext db =
                assertScope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

            int requestCountAfter = await db.IntakeRequests.CountAsync(request =>
                request.PatientId == patientId
            );

            Assert.Equal(requestCountBefore, requestCountAfter);
        }
    }

    [Fact]
    public async Task UpdateStatus_WhenRequestBelongsToAnotherClinic_ReturnsNotFound()
    {
        // Arrange
        int requestId;
        int ownerClinicId;
        int otherClinicId;
        RequestStatus originalStatus;
        RequestStatus attemptedStatus;

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            ClinicIntakeDbContext db =
                scope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

            IntakeRequest request = await db
                .IntakeRequests.AsNoTracking()
                .OrderBy(request => request.Id)
                .FirstAsync();

            requestId = request.Id;
            ownerClinicId = request.ClinicId;
            originalStatus = request.Status;

            otherClinicId = await db
                .Clinics.AsNoTracking()
                .Where(clinic => clinic.Id != ownerClinicId)
                .OrderBy(clinic => clinic.Id)
                .Select(clinic => clinic.Id)
                .FirstAsync();

            // Make sure the attempted status differs from
            // the request's current status.
            attemptedStatus =
                originalStatus == RequestStatus.Completed
                    ? RequestStatus.InReview
                    : RequestStatus.Completed;
        }

        using HttpClient otherClinicClient = CreateClientForClinic(otherClinicId);

        var dto = new UpdateRequestStatusDto { Status = attemptedStatus };

        // Act
        HttpResponseMessage response = await otherClinicClient.PutAsJsonAsync(
            $"/api/v1/requests/{requestId}/status",
            dto,
            JsonOptions
        );

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Confirm the database record was not changed.
        using (IServiceScope assertScope = _factory.Services.CreateScope())
        {
            ClinicIntakeDbContext db =
                assertScope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

            RequestStatus savedStatus = await db
                .IntakeRequests.AsNoTracking()
                .Where(request => request.Id == requestId)
                .Select(request => request.Status)
                .SingleAsync();

            Assert.Equal(originalStatus, savedStatus);
        }
    }

    [Fact]
    public async Task DeleteRequest_WhenRequestBelongsToAnotherClinic_ReturnsNotFound()
    {
        // Arrange
        int requestId;
        int ownerClinicId;
        int otherClinicId;

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            ClinicIntakeDbContext db =
                scope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

            IntakeRequest request = await db
                .IntakeRequests.AsNoTracking()
                .OrderBy(request => request.Id)
                .FirstAsync();

            requestId = request.Id;
            ownerClinicId = request.ClinicId;

            otherClinicId = await db
                .Clinics.AsNoTracking()
                .Where(clinic => clinic.Id != ownerClinicId)
                .OrderBy(clinic => clinic.Id)
                .Select(clinic => clinic.Id)
                .FirstAsync();
        }

        // CreateClientForClinic() uses demo-token,
        // which has the Admin role.
        using HttpClient otherClinicAdmin = CreateClientForClinic(otherClinicId);

        // Act
        HttpResponseMessage response = await otherClinicAdmin.DeleteAsync(
            $"/api/v1/requests/{requestId}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Confirm the record still exists.
        using (IServiceScope assertScope = _factory.Services.CreateScope())
        {
            ClinicIntakeDbContext db =
                assertScope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

            bool requestStillExists = await db
                .IntakeRequests.AsNoTracking()
                .AnyAsync(request => request.Id == requestId);

            Assert.True(requestStillExists);
        }
    }
}

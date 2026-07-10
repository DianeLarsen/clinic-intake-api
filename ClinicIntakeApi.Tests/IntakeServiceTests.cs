using ClinicIntakeApi.Models;
using ClinicIntakeApi.Repositories;
using ClinicIntakeApi.Services;
using Moq;

namespace ClinicIntakeApi.Tests;

public class IntakeServiceTests
{
    [Fact]
    public async Task AddRequestAsync_WhenPatientExists_ReturnsNewRequest()
    {
        // Arrange

        // Create a fake repository.
        var repositoryMock = new Mock<IIntakeRepository>();

        // Create a fake patient.
        var patient = new Patient
        {
            Id = 1,
            FirstName = "Diane",
            LastName = "Larsen",
            ClinicId = 10,
        };

        // Teach the fake repository:
        //
        // "sets up When someone asks for patient 1 (.Setup()),
        // return this patient (.ReturnsAsync())."
        repositoryMock
            .Setup(repository => repository.GetPatientByIdAsync(1))
            .ReturnsAsync(patient);

        // Tell the fake repository:
        // "When someone adds a request, return that request."
        repositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<IntakeRequest>()))
            .ReturnsAsync((IntakeRequest request) => request);

        // Give the fake repository to the real service.
        var service = new IntakeService(repositoryMock.Object);

        // Act

        IntakeRequest? result = await service.AddRequestAsync(1);

        // Assert

        Assert.NotNull(result);
        Assert.Equal(1, result.PatientId);
        Assert.Equal(10, result.ClinicId);
    }

    [Fact]
    public async Task AddRequestAsync_WhenPatientDoesNotExist_ReturnsNull()
    {
        // Arrange

        // Create a fake repository.
        var repositoryMock = new Mock<IIntakeRepository>();

        // Teach the fake repository:
        // "When someone asks for patient 999,
        // pretend the patient does not exist."
        repositoryMock
            .Setup(repository => repository.GetPatientByIdAsync(999))
            .ReturnsAsync((Patient?)null);

        // Create the service.
        var service = new IntakeService(repositoryMock.Object);

        // Act

        IntakeRequest? result = await service.AddRequestAsync(999);

        // Assert

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteRequestAsync_CallsRepositoryDeleteMethod()
    {
        // Arrange

        // Create a fake repository.
        var repositoryMock = new Mock<IIntakeRepository>();

        // Teach the fake repository:
        // "If DeleteAsync is called,
        // return true."
        repositoryMock.Setup(repository => repository.DeleteAsync(123)).ReturnsAsync(true);

        // Create the service.
        var service = new IntakeService(repositoryMock.Object);

        // Act

        await service.DeleteRequestAsync(123);

        // Assert

        repositoryMock.Verify(repository => repository.DeleteAsync(123), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenRequestExists_UpdatesStatus()
    {
        // Arrange

        // Create a fake repository.
        var repositoryMock = new Mock<IIntakeRepository>();

        // Create a real intake request.
        var request = new IntakeRequest
        {
            Id = 123,
            PatientId = 1,
            ClinicId = 10,
        };

        // Teach the fake repository:
        // "When someone asks for request 123,
        // return this request."
        repositoryMock.Setup(repository => repository.GetByIdAsync(123)).ReturnsAsync(request);

        // Create the service.
        var service = new IntakeService(repositoryMock.Object);

        // Act

        bool result = await service.UpdateStatusAsync(123, RequestStatus.Completed);

        // Assert

        Assert.True(result);

        Assert.Equal(RequestStatus.Completed, request.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenRequestDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repositoryMock = new Mock<IIntakeRepository>();

        repositoryMock
            .Setup(repository => repository.GetByIdAsync(123))
            .ReturnsAsync((IntakeRequest?)null);

        var service = new IntakeService(repositoryMock.Object);

        // Act
        bool result = await service.UpdateStatusAsync(123, RequestStatus.Completed);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetCompletedRequestsAsync_WhenRequestExists_ReturnsOnlyCompletedRequests()
    {
        // Arrange

        // Create a fake repository.
        var repositoryMock = new Mock<IIntakeRepository>();

        // Create a real intake request.
        var request1 = new IntakeRequest
        {
            Id = 123,
            PatientId = 1,
            ClinicId = 10,
        };

        var request2 = new IntakeRequest
        {
            Id = 124,
            PatientId = 2,
            ClinicId = 10,
        };

        var request3 = new IntakeRequest
        {
            Id = 125,
            PatientId = 3,
            ClinicId = 10,
        };

        request1.UpdateStatus(RequestStatus.Completed);
        request2.UpdateStatus(RequestStatus.Completed);

        // Put all three requests into a list.
        var requests = new List<IntakeRequest> { request1, request2, request3 };

        // Teach the fake repository:
        // "When GetAllAsync() is called,
        // return all three requests."
        repositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(requests);

        // Create the service.
        var service = new IntakeService(repositoryMock.Object);

        // Act

        var result = await service.GetCompletedRequestsAsync();

        // Turn the result into a list so it is easy to check.
        var completedRequests = result.ToList();

        // Assert

        // Only two requests should be returned.
        Assert.Equal(2, completedRequests.Count);

        // Every returned request should be Completed.
        Assert.All(
            completedRequests,
            request => Assert.Equal(RequestStatus.Completed, request.Status)
        );

        // The submitted request should not be included.
        Assert.DoesNotContain(completedRequests, request => request.Id == 125);
    }

    [Fact]
    public async Task GetCompletedRequestsAsync_WhenNoRequestsAreCompleted_ReturnsEmptyList()
    {
        // Arrange

        // Create a fake repository.
        var repositoryMock = new Mock<IIntakeRepository>();

        // Create a real intake request.
        var request1 = new IntakeRequest
        {
            Id = 123,
            PatientId = 1,
            ClinicId = 10,
        };

        var request2 = new IntakeRequest
        {
            Id = 124,
            PatientId = 2,
            ClinicId = 10,
        };

        var request3 = new IntakeRequest
        {
            Id = 125,
            PatientId = 3,
            ClinicId = 10,
        };

        // Put all three requests into a list.
        var requests = new List<IntakeRequest> { request1, request2, request3 };

        // Teach the fake repository:
        // "When GetAllAsync() is called,
        // return all three requests."
        repositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(requests);

        // Create the service.
        var service = new IntakeService(repositoryMock.Object);

        // Act

        var result = await service.GetCompletedRequestsAsync();

        // Assert

        // No requests should be returned because none are completed.
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRequestCountAsync_WhenRequestsExist_ReturnsCorrectCount()
    {
        // Arrange

        // Create a fake repository.
        var repositoryMock = new Mock<IIntakeRepository>();

        // Create some real intake requests.
        var request1 = new IntakeRequest
        {
            Id = 123,
            PatientId = 1,
            ClinicId = 10,
        };

        var request2 = new IntakeRequest
        {
            Id = 124,
            PatientId = 2,
            ClinicId = 10,
        };

        var request3 = new IntakeRequest
        {
            Id = 125,
            PatientId = 3,
            ClinicId = 10,
        };

        // Put all three requests into a list.
        var requests = new List<IntakeRequest> { request1, request2, request3 };

        // Teach the fake repository:
        // "When GetAllAsync() is called,
        // return all three requests."
        repositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(requests);

        // Create the service.
        var service = new IntakeService(repositoryMock.Object);

        // Act

        int result = await service.GetRequestCountAsync();

        // Assert

        Assert.Equal(3, result);
    }

    [Fact]
    public async Task GetRequestCountAsync_WhenNoRequestsExist_ReturnsZero()
    {
        // Arrange

        // Create a fake repository.
        var repositoryMock = new Mock<IIntakeRepository>();

        var requests = new List<IntakeRequest>();

        // Teach the fake repository:
        // "When someone asks for all requests,
        // return an empty list."
        repositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(requests);

        // Create the service.
        var service = new IntakeService(repositoryMock.Object);

        // Act

        int result = await service.GetRequestCountAsync();

        // Assert

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetRequestSummariesAsync_WhenFilteringByStatus_ReturnsMatchingRequests()
    {
        // Arrange

        // Create a fake repository.
        var repositoryMock = new Mock<IIntakeRepository>();

        // Create three real requests.
        var request1 = new IntakeRequest
        {
            Id = 123,
            PatientId = 1,
            ClinicId = 10,
        };

        var request2 = new IntakeRequest
        {
            Id = 124,
            PatientId = 2,
            ClinicId = 10,
        };

        var request3 = new IntakeRequest
        {
            Id = 125,
            PatientId = 3,
            ClinicId = 10,
        };

        // Make requests 1 and 3 completed.
        // Request 2 stays Submitted.
        request1.UpdateStatus(RequestStatus.Completed);
        request3.UpdateStatus(RequestStatus.Completed);

        var requests = new List<IntakeRequest> { request1, request2, request3 };

        // Teach the fake repository:
        // "When GetAllAsync() is called,
        // return these three requests."
        repositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(requests);

        var service = new IntakeService(repositoryMock.Object);

        // Act

        // Ask for only Completed requests.
        var result = await service.GetRequestSummariesAsync(
            status: RequestStatus.Completed,
            patient: null,
            sort: null,
            page: 1,
            pageSize: 10
        );

        var items = result.Items.ToList();

        // Assert

        // Two requests match the Completed filter.
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, items.Count);

        // The returned results should contain requests 123 and 125.
        Assert.Contains(items, item => item.Id == 123);
        Assert.Contains(items, item => item.Id == 125);

        // The Submitted request should not be returned.
        Assert.DoesNotContain(items, item => item.Id == 124);
    }

    [Fact]
    public async Task GetRequestSummariesAsync_WhenPaginating_ReturnsCorrectPage()
    {
        // Arrange

        // Create a fake repository.
        var repositoryMock = new Mock<IIntakeRepository>();

        // Create five requests.
        var request1 = new IntakeRequest
        {
            Id = 1,
            PatientId = 1,
            ClinicId = 10,
        };

        var request2 = new IntakeRequest
        {
            Id = 2,
            PatientId = 2,
            ClinicId = 10,
        };

        var request3 = new IntakeRequest
        {
            Id = 3,
            PatientId = 3,
            ClinicId = 10,
        };

        var request4 = new IntakeRequest
        {
            Id = 4,
            PatientId = 4,
            ClinicId = 10,
        };

        var request5 = new IntakeRequest
        {
            Id = 5,
            PatientId = 5,
            ClinicId = 10,
        };

        var requests = new List<IntakeRequest> { request1, request2, request3, request4, request5 };

        // Teach the fake repository:
        // "When GetAllAsync() is called,
        // return all five requests."
        repositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(requests);

        var service = new IntakeService(repositoryMock.Object);

        // Act

        // Ask for page 2.
        //
        // Each page holds 2 requests.
        //
        // Page 1 = requests 1 and 2
        // Page 2 = requests 3 and 4
        // Page 3 = request 5
        var result = await service.GetRequestSummariesAsync(
            status: null,
            patient: null,
            sort: null,
            page: 2,
            pageSize: 2
        );

        var items = result.Items.ToList();

        // Assert

        // There are five total requests.
        Assert.Equal(5, result.TotalCount);

        // Five requests split into pages of two means three pages.
        Assert.Equal(3, result.TotalPages);

        // We asked for page 2.
        Assert.Equal(2, result.Page);

        // Each page can hold two items.
        Assert.Equal(2, result.PageSize);

        // Page 2 should contain two requests.
        Assert.Equal(2, items.Count);

        // Page 2 should contain requests 3 and 4.
        Assert.Equal(3, items[0].Id);
        Assert.Equal(4, items[1].Id);
    }

    [Fact]
    public async Task GetRequestSummariesAsync_WhenFilteringByPatientName_ReturnsMatchingRequests()
    {
        // Arrange

        var repositoryMock = new Mock<IIntakeRepository>();

        // Create patients.
        var patient1 = new Patient
        {
            Id = 1,
            FirstName = "Diane",
            LastName = "Larsen",
            ClinicId = 10,
        };

        var patient2 = new Patient
        {
            Id = 2,
            FirstName = "Andrew",
            LastName = "Larsen",
            ClinicId = 10,
        };

        var patient3 = new Patient
        {
            Id = 3,
            FirstName = "Ella",
            LastName = "Smith",
            ClinicId = 10,
        };

        // Create requests and connect each request to a patient.
        var request1 = new IntakeRequest
        {
            Id = 101,
            PatientId = 1,
            ClinicId = 10,
            Patient = patient1,
        };

        var request2 = new IntakeRequest
        {
            Id = 102,
            PatientId = 2,
            ClinicId = 10,
            Patient = patient2,
        };

        var request3 = new IntakeRequest
        {
            Id = 103,
            PatientId = 3,
            ClinicId = 10,
            Patient = patient3,
        };

        var requests = new List<IntakeRequest> { request1, request2, request3 };

        repositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(requests);

        var service = new IntakeService(repositoryMock.Object);

        // Act

        // Search for "Larsen".
        var result = await service.GetRequestSummariesAsync(
            status: null,
            patient: "Larsen",
            sort: null,
            page: 1,
            pageSize: 10
        );

        var items = result.Items.ToList();

        // Assert

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, items.Count);

        Assert.Contains(items, item => item.Id == 101);
        Assert.Contains(items, item => item.Id == 102);

        Assert.DoesNotContain(items, item => item.Id == 103);
    }

    [Fact]
    public async Task GetRequestSummariesAsync_WhenSortingByName_ReturnsAlphabeticalOrder()
    {
        // Arrange

        var repositoryMock = new Mock<IIntakeRepository>();

        var patient1 = new Patient
        {
            Id = 1,
            FirstName = "Tyler",
            LastName = "Larsen",
            ClinicId = 10,
        };

        var patient2 = new Patient
        {
            Id = 2,
            FirstName = "Andrew",
            LastName = "Larsen",
            ClinicId = 10,
        };

        var patient3 = new Patient
        {
            Id = 3,
            FirstName = "Diane",
            LastName = "Larsen",
            ClinicId = 10,
        };

        var request1 = new IntakeRequest
        {
            Id = 101,
            PatientId = 1,
            ClinicId = 10,
            Patient = patient1,
        };

        var request2 = new IntakeRequest
        {
            Id = 102,
            PatientId = 2,
            ClinicId = 10,
            Patient = patient2,
        };

        var request3 = new IntakeRequest
        {
            Id = 103,
            PatientId = 3,
            ClinicId = 10,
            Patient = patient3,
        };

        var requests = new List<IntakeRequest> { request1, request2, request3 };

        repositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(requests);

        var service = new IntakeService(repositoryMock.Object);

        // Act

        var result = await service.GetRequestSummariesAsync(
            status: null,
            patient: null,
            sort: "name",
            page: 1,
            pageSize: 10
        );

        var items = result.Items.ToList();

        // Assert

        Assert.Equal(3, items.Count);

        Assert.Equal("Andrew Larsen", items[0].PatientName);
        Assert.Equal("Diane Larsen", items[1].PatientName);
        Assert.Equal("Tyler Larsen", items[2].PatientName);
    }

    [Fact]
    public async Task GetRequestSummariesAsync_WhenSortingByNameDescending_ReturnsReverseAlphabeticalOrder()
    {
        // Arrange
        var repositoryMock = new Mock<IIntakeRepository>();

        var patient1 = new Patient
        {
            Id = 1,
            FirstName = "Tyler",
            LastName = "Larsen",
            ClinicId = 10,
        };

        var patient2 = new Patient
        {
            Id = 2,
            FirstName = "Andrew",
            LastName = "Larsen",
            ClinicId = 10,
        };

        var patient3 = new Patient
        {
            Id = 3,
            FirstName = "Diane",
            LastName = "Larsen",
            ClinicId = 10,
        };

        var requests = new List<IntakeRequest>
        {
            new()
            {
                Id = 101,
                PatientId = 1,
                ClinicId = 10,
                Patient = patient1,
            },
            new()
            {
                Id = 102,
                PatientId = 2,
                ClinicId = 10,
                Patient = patient2,
            },
            new()
            {
                Id = 103,
                PatientId = 3,
                ClinicId = 10,
                Patient = patient3,
            },
        };

        repositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(requests);

        var service = new IntakeService(repositoryMock.Object);

        // Act
        var result = await service.GetRequestSummariesAsync(
            status: null,
            patient: null,
            sort: "name_desc",
            page: 1,
            pageSize: 10
        );

        var items = result.Items.ToList();

        // Assert
        Assert.Equal("Tyler Larsen", items[0].PatientName);
        Assert.Equal("Diane Larsen", items[1].PatientName);
        Assert.Equal("Andrew Larsen", items[2].PatientName);
    }

    [Fact]
    public async Task GetRequestSummariesAsync_WhenPatientAndClinicAreMissing_UsesUnknownNames()
    {
        // Arrange
        var repositoryMock = new Mock<IIntakeRepository>();

        var request = new IntakeRequest
        {
            Id = 101,
            PatientId = 1,
            ClinicId = 10,
            Patient = null,
            Clinic = null,
        };

        repositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(new List<IntakeRequest> { request });

        var service = new IntakeService(repositoryMock.Object);

        // Act
        var result = await service.GetRequestSummariesAsync(
            status: null,
            patient: null,
            sort: null,
            page: 1,
            pageSize: 10
        );

        var item = result.Items.Single();

        // Assert
        Assert.Equal("Unknown Patient", item.PatientName);
        Assert.Equal("Unknown Clinic", item.ClinicName);
        Assert.Equal("Unknown Patient - Submitted - Unknown Clinic", item.DisplayText);
    }

    [Fact]
    public async Task GetRequestSummariesAsync_WhenFilteringByPartialName_IgnoresCase()
    {
        // Arrange
        var repositoryMock = new Mock<IIntakeRepository>();

        var patient1 = new Patient
        {
            Id = 1,
            FirstName = "Andrew",
            LastName = "Larsen",
            ClinicId = 10,
        };

        var patient2 = new Patient
        {
            Id = 2,
            FirstName = "Ella",
            LastName = "Smith",
            ClinicId = 10,
        };

        var requests = new List<IntakeRequest>
        {
            new()
            {
                Id = 101,
                PatientId = 1,
                ClinicId = 10,
                Patient = patient1,
            },
            new()
            {
                Id = 102,
                PatientId = 2,
                ClinicId = 10,
                Patient = patient2,
            },
        };

        repositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(requests);

        var service = new IntakeService(repositoryMock.Object);

        // Act
        var result = await service.GetRequestSummariesAsync(
            status: null,
            patient: "AND",
            sort: null,
            page: 1,
            pageSize: 10
        );

        var items = result.Items.ToList();

        // Assert
        Assert.Single(items);
        Assert.Equal("Andrew Larsen", items[0].PatientName);
    }
}

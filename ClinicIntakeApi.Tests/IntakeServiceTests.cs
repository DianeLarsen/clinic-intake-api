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
}

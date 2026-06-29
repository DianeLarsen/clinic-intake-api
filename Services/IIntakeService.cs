// Services/IIntakeService.cs
using ClinicIntakeApi.Models;

namespace ClinicIntakeApi.Services;

public interface IIntakeService
{
    IntakeRequest AddRequest(string patientName);

    IntakeRequest? FindRequestById(int id);

    bool UpdateStatus(int id, RequestStatus status);

    int GetRequestCount();

    IEnumerable<IntakeRequest> GetAllRequests();

    IEnumerable<IntakeRequest> GetCompletedRequests();

    IEnumerable<IntakeRequest> GetRequests(RequestStatus? status);

    bool DeleteRequest(int id);
}

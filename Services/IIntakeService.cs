// Services/IIntakeService.cs
using ClinicIntakeApi.Models;
using ClinicIntakeApi.Dtos;

namespace ClinicIntakeApi.Services;

public interface IIntakeService
{
    Task<IntakeRequest> AddRequestAsync(string patientName);

    Task<IntakeRequest?> FindRequestByIdAsync(int id);

    Task<bool> UpdateStatusAsync(int id, RequestStatus status);

    Task<int> GetRequestCountAsync();

    Task<IEnumerable<IntakeRequest>> GetAllRequestsAsync();

    Task<IEnumerable<IntakeRequest>> GetCompletedRequestsAsync();

    Task<IEnumerable<IntakeRequest>> GetRequestsAsync(
        RequestStatus? status,
        string? patient,
        string? sort,
        int page,
        int pageSize
    );

    Task<bool> DeleteRequestAsync(int id);

    Task<IEnumerable<RequestSummaryDto>> GetRequestSummariesAsync(
        RequestStatus? status,
        string? patient,
        string? sort,
        int page,
        int pageSize
    );
}

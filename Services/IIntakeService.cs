// Services/IIntakeService.cs
using ClinicIntakeApi.Dtos;
using ClinicIntakeApi.Models;

namespace ClinicIntakeApi.Services;

public interface IIntakeService
{
    Task<IntakeRequest?> AddRequestAsync(int patientId, int clinicId);

    Task<IntakeRequest?> FindRequestByIdAsync(int id, int clinicId);

    Task<bool> UpdateStatusAsync(int id, RequestStatus status, int clinicId);

    Task<int> GetRequestCountAsync(int clinicId);

    Task<IEnumerable<IntakeRequest>> GetAllRequestsAsync(int clinicId);

    Task<IEnumerable<IntakeRequest>> GetCompletedRequestsAsync(int clinicId);

    Task<bool> DeleteRequestAsync(int id, int clinicId);

    Task<PagedResponse<RequestSummaryDto>> GetRequestSummariesAsync(
        RequestStatus? status,
        string? patient,
        string? sort,
        int page,
        int pageSize,
        int clinicId
    );
}

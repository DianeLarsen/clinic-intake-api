// Services/IntakeService.cs
using ClinicIntakeApi.Dtos;
using ClinicIntakeApi.Models;
using ClinicIntakeApi.Repositories;

namespace ClinicIntakeApi.Services;

public class IntakeService : IIntakeService
{
    private readonly IIntakeRepository _repository;

    public IntakeService(IIntakeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IntakeRequest?> AddRequestAsync(int patientId)
    {
        Patient? patient = await _repository.GetPatientByIdAsync(patientId);

        if (patient is null)
        {
            return null;
        }

        IntakeRequest request = new() { PatientId = patient.Id, ClinicId = patient.ClinicId };

        return await _repository.AddAsync(request);
    }

    public async Task<IntakeRequest?> FindRequestByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateStatusAsync(int id, RequestStatus status)
    {
        IntakeRequest? request = await FindRequestByIdAsync(id);

        if (request is null)
        {
            return false;
        }

        request.UpdateStatus(status);

        return await _repository.UpdateAsync(request);
    }

    public async Task<int> GetRequestCountAsync()
    {
        var requests = await _repository.GetAllAsync();
        return requests.Count();
    }

    public async Task<IEnumerable<IntakeRequest>> GetAllRequestsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<IntakeRequest>> GetCompletedRequestsAsync()
    {
        var requests = await _repository.GetAllAsync();
        return requests.Where(request => request.Status == RequestStatus.Completed);
    }

    private async Task<IEnumerable<IntakeRequest>> BuildRequestQueryAsync(
        RequestStatus? status,
        string? patient,
        string? sort
    )
    {
        IEnumerable<IntakeRequest> requests = await _repository.GetAllAsync();

        if (status is not null)
        {
            requests = requests.Where(r => r.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(patient))
        {
            requests = requests.Where(r =>
                r.Patient is not null
                && r.Patient.GetFullName().Contains(patient, StringComparison.OrdinalIgnoreCase)
            );
        }

        if (sort == "name")
        {
            requests = requests.OrderBy(r => r.Patient?.GetFullName());
        }

        if (sort == "name_desc")
        {
            requests = requests.OrderByDescending(r => r.Patient?.GetFullName());
        }

        return requests;
    }

    public async Task<PagedResponse<RequestSummaryDto>> GetRequestSummariesAsync(
        RequestStatus? status,
        string? patient,
        string? sort,
        int page,
        int pageSize
    )
    {
        IEnumerable<IntakeRequest> requests = await BuildRequestQueryAsync(status, patient, sort);

        int totalCount = requests.Count();

        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        IEnumerable<RequestSummaryDto> items = requests
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r =>
            {
                string patientName = r.Patient?.GetFullName() ?? "Unknown Patient";

                string clinicName = r.Clinic?.Name ?? "Unknown Clinic";

                return new RequestSummaryDto
                {
                    Id = r.Id,
                    PatientName = patientName,
                    ClinicName = clinicName,
                    DisplayText = $"{patientName} - {r.Status} - {clinicName}",
                };
            });
        return new PagedResponse<RequestSummaryDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Items = items,
        };
    }

    public async Task<bool> DeleteRequestAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}

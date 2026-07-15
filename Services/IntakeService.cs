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

    public async Task<IntakeRequest?> AddRequestAsync(int patientId, int clinicId)
    {
        // Find the patient only within the authenticated clinic.
        Patient? patient = await _repository.GetPatientByIdAsync(patientId, clinicId);

        if (patient is null)
        {
            return null;
        }

        // The clinic comes from the trusted JWT claim,
        // not from client-supplied JSON.
        IntakeRequest request = new() { PatientId = patient.Id, ClinicId = clinicId };

        return await _repository.AddAsync(request, clinicId);
    }

    public async Task<IntakeRequest?> FindRequestByIdAsync(int id, int clinicId)
    {
        return await _repository.GetByIdAsync(id, clinicId);
    }

    public async Task<bool> UpdateStatusAsync(int id, RequestStatus status, int clinicId)
    {
        IntakeRequest? request = await FindRequestByIdAsync(id, clinicId);

        if (request is null)
        {
            return false;
        }

        request.UpdateStatus(status);

        return await _repository.UpdateAsync(request, clinicId);
    }

    public async Task<int> GetRequestCountAsync(int clinicId)
    {
        IEnumerable<IntakeRequest> requests = await _repository.GetAllAsync(clinicId);

        return requests.Count();
    }

    public async Task<IEnumerable<IntakeRequest>> GetAllRequestsAsync(int clinicId)
    {
        return await _repository.GetAllAsync(clinicId);
    }

    public async Task<IEnumerable<IntakeRequest>> GetCompletedRequestsAsync(int clinicId)
    {
        IEnumerable<IntakeRequest> requests = await _repository.GetAllAsync(clinicId);

        return requests.Where(request => request.Status == RequestStatus.Completed);
    }

    private async Task<IEnumerable<IntakeRequest>> BuildRequestQueryAsync(
        RequestStatus? status,
        string? patient,
        string? sort,
        int clinicId
    )
    {
        // The initial collection already contains only
        // records belonging to this clinic.
        IEnumerable<IntakeRequest> requests = await _repository.GetAllAsync(clinicId);

        if (status is not null)
        {
            requests = requests.Where(request => request.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(patient))
        {
            requests = requests.Where(request =>
                request.Patient is not null
                && request
                    .Patient.GetFullName()
                    .Contains(patient, StringComparison.OrdinalIgnoreCase)
            );
        }

        if (sort == "name")
        {
            requests = requests.OrderBy(request => request.Patient?.GetFullName());
        }

        if (sort == "name_desc")
        {
            requests = requests.OrderByDescending(request => request.Patient?.GetFullName());
        }

        return requests;
    }

    public async Task<PagedResponse<RequestSummaryDto>> GetRequestSummariesAsync(
        RequestStatus? status,
        string? patient,
        string? sort,
        int page,
        int pageSize,
        int clinicId
    )
    {
        IEnumerable<IntakeRequest> requests = await BuildRequestQueryAsync(
            status,
            patient,
            sort,
            clinicId
        );

        int totalCount = requests.Count();

        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        IEnumerable<RequestSummaryDto> items = requests
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(request =>
            {
                string patientName = request.Patient?.GetFullName() ?? "Unknown Patient";

                string clinicName = request.Clinic?.Name ?? "Unknown Clinic";

                return new RequestSummaryDto
                {
                    Id = request.Id,
                    PatientName = patientName,
                    ClinicName = clinicName,
                    DisplayText = $"{patientName} - {request.Status} - {clinicName}",
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

    public async Task<bool> DeleteRequestAsync(int id, int clinicId)
    {
        return await _repository.DeleteAsync(id, clinicId);
    }
}

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

    public async Task<IntakeRequest> AddRequestAsync(string patientName)
    {
        if (string.IsNullOrWhiteSpace(patientName))
        {
            throw new ArgumentException("Patient name is required.");
        }

        IntakeRequest request = new IntakeRequest(patientName);

        return await _repository.AddAsync(request);
    }

    public async Task<IntakeRequest?> FindRequestByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateStatusAsync(int id, RequestStatus status)
    {
        IntakeRequest? request = await FindRequestByIdAsync(id);

        request?.UpdateStatus(status);

        return request is not null;
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
                r.PatientName.Contains(patient, StringComparison.OrdinalIgnoreCase)
            );
        }

        if (sort == "name")
        {
            requests = requests.OrderBy(r => r.PatientName);
        }

        if (sort == "name_desc")
        {
            requests = requests.OrderByDescending(r => r.PatientName);
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
            .Select(r => new RequestSummaryDto
            {
                Id = r.Id,
                DisplayText = $"{r.PatientName} - {r.Status}",
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

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

    public IntakeRequest AddRequest(string patientName)
    {
        if (string.IsNullOrWhiteSpace(patientName))
        {
            throw new ArgumentException("Patient name is required.");
        }

        IntakeRequest request = new IntakeRequest(patientName);

        return _repository.Add(request);
    }

    public IntakeRequest? FindRequestById(int id)
    {
        return _repository.GetById(id);
    }

    public bool UpdateStatus(int id, RequestStatus status)
    {
        IntakeRequest? request = FindRequestById(id);

        request?.UpdateStatus(status);

        return request is not null;
    }

    public int GetRequestCount()
    {
        return _repository.GetAll().Count();
    }

    public IEnumerable<IntakeRequest> GetAllRequests()
    {
        return _repository.GetAll();
    }

    public IEnumerable<IntakeRequest> GetCompletedRequests()
    {
        return _repository.GetAll().Where(request => request.Status == RequestStatus.Completed);
    }

    public IEnumerable<IntakeRequest> GetRequests(
        RequestStatus? status,
        string? patient,
        string? sort,
        int page,
        int pageSize
    )
    {
        IEnumerable<IntakeRequest> requests = _repository.GetAll();

        // Filter by status
        if (status is not null)
        {
            requests = requests.Where(r => r.Status == status.Value);
        }

        // Filter by patient name
        if (!string.IsNullOrWhiteSpace(patient))
        {
            requests = requests.Where(r =>
                r.PatientName.Contains(patient, StringComparison.OrdinalIgnoreCase)
            );
        }

        // Sort
        if (sort == "name")
        {
            requests = requests.OrderBy(r => r.PatientName);
        }

        if (sort == "name_desc")
        {
            requests = requests.OrderByDescending(r => r.PatientName);
        }

        requests = requests.Skip((page - 1) * pageSize).Take(pageSize);

        return requests;
    }

    public IEnumerable<RequestSummaryDto> GetRequestSummaries(
        RequestStatus? status,
        string? patient,
        string? sort,
        int page,
        int pageSize
    )
    {
        IEnumerable<IntakeRequest> requests = GetRequests(status, patient, sort, page, pageSize);

        return requests.Select(r => new RequestSummaryDto
        {
            Id = r.Id,
            DisplayText = $"{r.PatientName} - {r.Status}",
        });
    }

    public bool DeleteRequest(int id)
    {
        return _repository.Delete(id);
    }
}

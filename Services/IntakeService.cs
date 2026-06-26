// Services/IntakeService.cs
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

    public bool DeleteRequest(int id)
    {
        return _repository.Delete(id);
    }
}

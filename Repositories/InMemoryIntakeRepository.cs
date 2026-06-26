// Repositories/InMemoryIntakeRepository.cs
using ClinicIntakeApi.Models;

namespace ClinicIntakeApi.Repositories;

public class InMemoryIntakeRepository : IIntakeRepository
{
    private readonly List<IntakeRequest> _requests = [];

    public IntakeRequest Add(IntakeRequest request)
    {
        _requests.Add(request);
        return request;
    }

    public IEnumerable<IntakeRequest> GetAll()
    {
        return _requests;
    }

    public IntakeRequest? GetById(int id)
    {
        return _requests.FirstOrDefault(r => r.Id == id);
    }

    public bool Delete(int id)
    {
        IntakeRequest? request = GetById(id);

        if (request is null)
        {
            return false;
        }

        return _requests.Remove(request);
    }
}

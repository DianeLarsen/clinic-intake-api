// Repositories/InMemoryIntakeRepository.cs
using ClinicIntakeApi.Models;

namespace ClinicIntakeApi.Repositories;

public class InMemoryIntakeRepository
    : IIntakeRepository
{
    private readonly List<IntakeRequest> _requests = [];
    private int _nextId = 1;

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
        return _requests.FirstOrDefault(
            r => r.Id == id);
    }

     public int GetNextId()
    {
        return _nextId++;
    }
}
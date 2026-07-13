// Repositories/InMemoryIntakeRepository.cs
using ClinicIntakeApi.Models;

namespace ClinicIntakeApi.Repositories;

public class InMemoryIntakeRepository : IIntakeRepository
{
    private readonly List<IntakeRequest> _requests = [];

    private readonly List<Patient> _patients =
    [
        new Patient
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Johnson",
            DateOfBirth = new DateOnly(1984, 3, 12),
            ClinicId = 1,
        },
        new Patient
        {
            Id = 2,
            FirstName = "Bob",
            LastName = "Smith",
            DateOfBirth = new DateOnly(1979, 7, 22),
            ClinicId = 1,
        },
    ];

    public async Task<IntakeRequest> AddAsync(IntakeRequest request)
    {
        _requests.Add(request);
        return await Task.FromResult(request);
    }

    public async Task<IEnumerable<IntakeRequest>> GetAllAsync()
    {
        return await Task.FromResult(_requests);
    }

    public async Task<IntakeRequest?> GetByIdAsync(int id)
    {
        return await Task.FromResult(_requests.FirstOrDefault(r => r.Id == id));
    }


    public async Task<bool> UpdateAsync(IntakeRequest request)
    {
        // In-memory repository doesn't have a real database,
        // so we just pretend the update worked.
        return await Task.FromResult(true);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        IntakeRequest? request = await GetByIdAsync(id);

        if (request is null)
        {
            return false;
        }

        return await Task.FromResult(_requests.Remove(request));
    }

    public Task<Patient?> GetPatientByIdAsync(int patientId)
    {
        Patient? patient = _patients.FirstOrDefault(p => p.Id == patientId);

        return Task.FromResult(patient);
    }
}

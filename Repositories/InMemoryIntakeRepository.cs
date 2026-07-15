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

    public Task<IntakeRequest> AddAsync(IntakeRequest request, int clinicId)
    {
        // Refuse to add a record belonging to another clinic.
        if (request.ClinicId != clinicId)
        {
            throw new InvalidOperationException("Cannot add a request for another clinic.");
        }

        _requests.Add(request);

        return Task.FromResult(request);
    }

    public Task<IEnumerable<IntakeRequest>> GetAllAsync(int clinicId)
    {
        IEnumerable<IntakeRequest> requests = _requests
            .Where(request => request.ClinicId == clinicId)
            .ToList();

        return Task.FromResult(requests);
    }

    public Task<IntakeRequest?> GetByIdAsync(int id, int clinicId)
    {
        IntakeRequest? request = _requests.FirstOrDefault(request =>
            request.Id == id && request.ClinicId == clinicId
        );

        return Task.FromResult(request);
    }

    public Task<bool> UpdateAsync(IntakeRequest request, int clinicId)
    {
        // The request must belong to the authenticated clinic
        // and must already exist in this repository.
        bool exists =
            request.ClinicId == clinicId
            && _requests.Any(existingRequest =>
                existingRequest.Id == request.Id && existingRequest.ClinicId == clinicId
            );

        // The object stored in the list is already updated
        // because the service modified that same object.
        return Task.FromResult(exists);
    }

    public async Task<bool> DeleteAsync(int id, int clinicId)
    {
        IntakeRequest? request = await GetByIdAsync(id, clinicId);

        if (request is null)
        {
            return false;
        }

        return _requests.Remove(request);
    }

    public Task<Patient?> GetPatientByIdAsync(int patientId, int clinicId)
    {
        Patient? patient = _patients.FirstOrDefault(patient =>
            patient.Id == patientId && patient.ClinicId == clinicId
        );

        return Task.FromResult(patient);
    }
}

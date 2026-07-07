// Repositories/IIntakeRepository.cs
using ClinicIntakeApi.Models;

namespace ClinicIntakeApi.Repositories;

public interface IIntakeRepository
{
    Task<IntakeRequest> AddAsync(IntakeRequest request);

    Task<IEnumerable<IntakeRequest>> GetAllAsync();

    Task<IntakeRequest?> GetByIdAsync(int id);

    Task<bool> DeleteAsync(int id);

    Task<Patient?> GetPatientByIdAsync(int patientId);
}

// Repositories/IIntakeRepository.cs
using ClinicIntakeApi.Models;

namespace ClinicIntakeApi.Repositories;

public interface IIntakeRepository
{
    Task<IntakeRequest> AddAsync(IntakeRequest request, int clinicId);

    Task<IEnumerable<IntakeRequest>> GetAllAsync(int clinicId);

    Task<IntakeRequest?> GetByIdAsync(int id, int clinicId);

    Task<bool> UpdateAsync(IntakeRequest request, int clinicId);

    Task<bool> DeleteAsync(int id, int clinicId);

    Task<Patient?> GetPatientByIdAsync(int patientId, int clinicId);
}

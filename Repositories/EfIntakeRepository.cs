using ClinicIntakeApi.Data;
using ClinicIntakeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicIntakeApi.Repositories;

public class EfIntakeRepository : IIntakeRepository
{
    private readonly ClinicIntakeDbContext _db;

    public EfIntakeRepository(ClinicIntakeDbContext db)
    {
        _db = db;
    }

    public async Task<IntakeRequest> AddAsync(IntakeRequest request, int clinicId)
    {
        // The record must belong to the clinic identified
        // by the authenticated user's JWT.
        if (request.ClinicId != clinicId)
        {
            throw new InvalidOperationException("Cannot add a request for another clinic.");
        }

        _db.IntakeRequests.Add(request);
        await _db.SaveChangesAsync();

        return request;
    }

    public async Task<IEnumerable<IntakeRequest>> GetAllAsync(int clinicId)
    {
        return await _db
            .IntakeRequests
            // Only retrieve records owned by this clinic.
            .Where(request => request.ClinicId == clinicId)
            .Include(request => request.Patient)
            .Include(request => request.Clinic)
            .ToListAsync();
    }

    public async Task<IntakeRequest?> GetByIdAsync(int id, int clinicId)
    {
        return await _db.IntakeRequests.FirstOrDefaultAsync(request =>
            request.Id == id && request.ClinicId == clinicId
        );
    }

    public async Task<bool> UpdateAsync(IntakeRequest request, int clinicId)
    {
        // Refuse to save an entity belonging to another clinic.
        if (request.ClinicId != clinicId)
        {
            return false;
        }

        // The request was loaded by GetByIdAsync(), so EF Core
        // is already tracking its changed status.
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id, int clinicId)
    {
        // Search using both the record ID and clinic ID.
        IntakeRequest? request = await GetByIdAsync(id, clinicId);

        if (request is null)
        {
            return false;
        }

        _db.IntakeRequests.Remove(request);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<Patient?> GetPatientByIdAsync(int patientId, int clinicId)
    {
        return await _db.Patients.FirstOrDefaultAsync(patient =>
            patient.Id == patientId && patient.ClinicId == clinicId
        );
    }
}

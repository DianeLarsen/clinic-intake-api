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

    public async Task<IntakeRequest> AddAsync(IntakeRequest request)
    {
        _db.IntakeRequests.Add(request);
        await _db.SaveChangesAsync();

        return request;
    }

    public async Task<IEnumerable<IntakeRequest>> GetAllAsync()
    {
        return await _db
            .IntakeRequests.Include(r => r.Patient)
            .Include(r => r.Clinic)
            .ToListAsync();
    }

    public async Task<IntakeRequest?> GetByIdAsync(int id)
    {
        return await _db.IntakeRequests.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> UpdateAsync(IntakeRequest request)
    {
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        IntakeRequest? request = await GetByIdAsync(id);

        if (request is null)
        {
            return false;
        }

        _db.IntakeRequests.Remove(request);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<Patient?> GetPatientByIdAsync(int patientId)
    {
        return await _db.Patients.FirstOrDefaultAsync(p => p.Id == patientId);
    }
}

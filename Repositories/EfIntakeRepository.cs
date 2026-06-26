using ClinicIntakeApi.Data;
using ClinicIntakeApi.Models;

namespace ClinicIntakeApi.Repositories;

public class EfIntakeRepository : IIntakeRepository
{
    private readonly ClinicIntakeDbContext _db;

    public EfIntakeRepository(ClinicIntakeDbContext db)
    {
        _db = db;
    }

    public IntakeRequest Add(IntakeRequest request)
    {
        _db.IntakeRequests.Add(request);
        _db.SaveChanges();

        return request;
    }

    public IEnumerable<IntakeRequest> GetAll()
    {
        return _db.IntakeRequests.ToList();
    }

    public IntakeRequest? GetById(int id)
    {
        return _db.IntakeRequests.FirstOrDefault(r => r.Id == id);
    }

    public bool Delete(int id)
    {
        IntakeRequest? request = GetById(id);

        if (request is null)
        {
            return false;
        }

        _db.IntakeRequests.Remove(request);
        _db.SaveChanges();

        return true;
    }
}

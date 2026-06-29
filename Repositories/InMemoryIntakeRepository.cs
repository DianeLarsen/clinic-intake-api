// Repositories/InMemoryIntakeRepository.cs
using ClinicIntakeApi.Models;

namespace ClinicIntakeApi.Repositories;

public class InMemoryIntakeRepository : IIntakeRepository
{
    private readonly List<IntakeRequest> _requests = [];

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

    public async Task<bool> DeleteAsync(int id)
    {
        IntakeRequest? request = await GetByIdAsync(id);

        if (request is null)
        {
            return false;
        }

        return await Task.FromResult(_requests.Remove(request));
    }
}

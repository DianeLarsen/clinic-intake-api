// Repositories/IIntakeRepository.cs
using ClinicIntakeApi.Models;

namespace ClinicIntakeApi.Repositories;

public interface IIntakeRepository
{
    IntakeRequest Add(IntakeRequest request);

    IEnumerable<IntakeRequest> GetAll();

    IntakeRequest? GetById(int id);

    bool Delete(int id);
}

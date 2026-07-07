namespace ClinicIntakeApi.Models;

public class Patient
{
    public int Id { get; set; }

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public DateOnly DateOfBirth { get; set; }

    public int ClinicId { get; set; }

    public Clinic? Clinic { get; set; }

    public List<IntakeRequest> IntakeRequests { get; set; } = [];

    public string GetFullName()
    {
        return $"{FirstName} {LastName}";
    }
}

namespace ClinicIntakeApi.Models;

public class Clinic
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public List<IntakeRequest> Requests { get; set; } = [];

    public List<Patient> Patients { get; set; } = [];
}

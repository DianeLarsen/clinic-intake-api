namespace ClinicIntakeApi.Models;

public class Patient
{
    public int Id { get; set; }

    public string FullName { get; set; } = "";

    // Required relationship:
    // every patient belongs to one clinic.
    public int ClinicId { get; set; }

    // Navigation property:
    // lets C# access patient.Clinic.
    public Clinic? Clinic { get; set; }

    // One patient can have many intake requests.
    public List<IntakeRequest> IntakeRequests { get; set; } = [];
}

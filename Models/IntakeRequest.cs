namespace ClinicIntakeApi.Models;

public class IntakeRequest
{
    public int Id { get; set; }

    public string PatientName { get; set; } = "";

    public RequestStatus Status { get; private set; } = RequestStatus.Submitted;

    public int ClinicId { get; set; }

    public Clinic? Clinic { get; set; }

    public int PatientId { get; set; }

    public Patient? Patient { get; set; }

    private IntakeRequest() { }

    public IntakeRequest(string patientName)
    {
        PatientName = patientName;
        Status = RequestStatus.Submitted;
    }

    public void UpdateStatus(RequestStatus status)
    {
        Status = status;
    }

    public string GetSummary()
    {
        return $"{Id}: {PatientName} ({Status})";
    }
}

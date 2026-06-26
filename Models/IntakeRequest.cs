namespace ClinicIntakeApi.Models;

public class IntakeRequest
{
    public int Id { get; set; }

    public string PatientName { get; set; } = "";

    public RequestStatus Status { get; private set; } = RequestStatus.Submitted;

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

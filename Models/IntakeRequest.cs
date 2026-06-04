// Models/IntakeRequest.cs
namespace ClinicIntakeApi.Models;


public class IntakeRequest
{
    public int Id { get; }

    public string PatientName { get; }

    public RequestStatus Status { get; private set; }

    public IntakeRequest(int id, string patientName)
    {
        Id = id;
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
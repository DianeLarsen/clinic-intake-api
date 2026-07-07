namespace ClinicIntakeApi.Models;

public class IntakeRequest
{
    public int Id { get; set; }

    //
    // Every intake request belongs to one clinic.
    //
    public int ClinicId { get; set; }

    public Clinic? Clinic { get; set; }

    //
    // Every intake request belongs to one patient.
    //
    public int PatientId { get; set; }

    public Patient? Patient { get; set; }

    //
    // Current workflow status.
    //
    // New requests start as Submitted by default.
    //
    public RequestStatus Status { get; private set; } = RequestStatus.Submitted;

    //
    // Public constructor used by both EF Core and the application.
    //
    public IntakeRequest() { }

    //
    // Update the request's workflow status.
    //
    public void UpdateStatus(RequestStatus status)
    {
        Status = status;
    }

    //
    // Returns a simple summary for debugging.
    //
    public string GetSummary()
    {
        return $"{Id}: {Patient?.GetFullName() ?? "Unknown Patient"} ({Status})";
    }
}

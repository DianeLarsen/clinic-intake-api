namespace ClinicIntakeApi.Models;

public class RequestStatusHistory
{
    public int Id { get; set; }

    public int IntakeRequestId { get; set; }

    public IntakeRequest? IntakeRequest { get; set; }

    public RequestStatus PreviousStatus { get; set; }

    public RequestStatus NewStatus { get; set; }
    public string? UpdatedBy { get; set; }

    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    //
    // Public constructor used by both EF Core and the application.
    //
    public RequestStatusHistory() { }


    //
    // Returns a simple summary for debugging.
    //
    public string GetSummary()
    {
        return $"{Id}: {IntakeRequest?.Patient?.GetFullName() ?? "Unknown Patient"} " +
       $"({PreviousStatus} -> {NewStatus}) by {UpdatedBy ?? "Unknown User"} at {ChangedAtUtc:u}";
    }
}

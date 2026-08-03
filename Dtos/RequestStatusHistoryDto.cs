namespace ClinicIntakeApi.Dtos;

public class RequestStatusHistoryDto
{
    public int Id { get; set; }

    public string PreviousStatus { get; set; } = "";

    public string NewStatus { get; set; } = "";

    public string UpdatedBy { get; set; } = "";

    public DateTime ChangedAtUtc { get; set; }
}

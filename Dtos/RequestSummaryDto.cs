namespace ClinicIntakeApi.Dtos;

public class RequestSummaryDto
{
    public int Id { get; set; }

    public string DisplayText { get; set; } = "";

    public string PatientName { get; set; } = "";

    public string ClinicName { get; set; } = "";
}

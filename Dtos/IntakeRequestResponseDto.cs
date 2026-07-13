using ClinicIntakeApi.Models;

namespace ClinicIntakeApi.Dtos;

public class IntakeRequestResponseDto
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int ClinicId { get; set; }

    public RequestStatus Status { get; set; }
}

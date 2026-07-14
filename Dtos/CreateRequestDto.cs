using System.ComponentModel.DataAnnotations;

namespace ClinicIntakeApi.Dtos;

public class CreateRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "PatientId must be greater than 0.")]
    public int PatientId { get; set; }
}

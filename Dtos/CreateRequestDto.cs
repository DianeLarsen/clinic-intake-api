using System.ComponentModel.DataAnnotations;

namespace ClinicIntakeApi.Dtos;

public class CreateRequestDto
{
    [Required]
    public int PatientId { get; set; }
}

namespace ClinicIntakeApi.Dtos;

using System.ComponentModel.DataAnnotations;

public class CreateRequestDto
{
    [Required]
    [StringLength(100)]
    public string PatientName { get; set; } = "";
}

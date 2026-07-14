using System.ComponentModel.DataAnnotations;
using ClinicIntakeApi.Models;

namespace ClinicIntakeApi.Dtos;

public class UpdateRequestStatusDto
{
    [EnumDataType(typeof(RequestStatus))]
    public RequestStatus Status { get; set; }
}

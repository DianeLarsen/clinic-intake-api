using Asp.Versioning;
using ClinicIntakeApi.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicIntakeApi.Controllers;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/system")]
// By default, endpoints in this controller require authentication.
[Authorize]
public class SystemController : ControllerBase
{
    // Overrides [Authorize] for this specific endpoint.
    // Anyone can check whether the API is running,
    // even when they do not provide an authentication token.
    [AllowAnonymous]
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "Healthy" });
    }
}

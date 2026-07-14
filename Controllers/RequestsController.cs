using Asp.Versioning;
using ClinicIntakeApi.Configuration;
using ClinicIntakeApi.Dtos;
using ClinicIntakeApi.Models;
using ClinicIntakeApi.Services;
using ClinicIntakeApi.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ClinicIntakeApi.Controllers;

//
// [ApiController]
//
// Marks this class as an ASP.NET Web API controller.
//
// This enables several API features automatically:
//
// • Better parameter binding
// • Automatic validation support
// • Automatic 400 Bad Request responses (when using model validation)
// • API-specific behaviors
//
[ApiController]
[ApiVersion(ApiVersions.V1)]
//
// [Route("[controller]")]
//
// Defines the base URL for every endpoint in this controller.
//
// "[controller]" is replaced with the class name,
// minus the word "Controller".
//
// RequestsController
//          ↓
//      /requests
//
[Route("api/v{version:apiVersion}/[controller]")]
// Requires the request to contain valid authentication.
// If authentication does not create a valid user,
// ASP.NET stops the request and returns 401 Unauthorized.
[Authorize]
//
// Controllers inherit from ControllerBase.
//
// ControllerBase provides many helper methods such as:
//
// Ok()
// NotFound()
// Created()
// BadRequest()
// NoContent()
//
// Instead of writing:
//
// Results.Ok(...)
//
// we can simply write:
//
// Ok(...)
//
public class RequestsController : ControllerBase
{
    //
    // Store the service in a private field.
    //
    // readonly means once the constructor sets it,
    // it cannot be changed.
    //
    private readonly IIntakeService _intakeService;

    // Contains the pagination settings loaded
    // from the "Pagination" configuration section.
    private readonly PaginationOptions _paginationOptions;

    //
    // Constructor
    //
    // ASP.NET's Dependency Injection system automatically
    // creates an IntakeService and passes it here.
    //
    // We save it so every action in this controller can use it.
    //
    public RequestsController(
        IIntakeService intakeService,
        IOptions<PaginationOptions> paginationOptions
    )
    {
        _intakeService = intakeService;

        // IOptions<T> is a wrapper provided by ASP.NET.
        // .Value retrieves the actual PaginationOptions object.
        _paginationOptions = paginationOptions.Value;
    }

    //
    // GET /requests
    //
    // Returns a paginated list of intake requests.
    //
    // Query string parameters are automatically bound
    // by ASP.NET.
    //
    // Example:
    //
    // /requests?page=2&pageSize=10&status=Completed
    //
    // becomes:
    //
    // page = 2
    // pageSize = 10
    // status = Completed
    //
    [HttpGet]
    public async Task<IActionResult> Get(
        RequestStatus? status,
        string? patient,
        string? sort,
        int page = 1,
        int? pageSize = null
    )
    {
        // If the client did not provide pageSize,
        // use the configured default.
        int resolvedPageSize = pageSize ?? _paginationOptions.DefaultPageSize;

        // Page numbers start at 1.
        if (page < 1)
        {
            return BadRequest("Page must be greater than or equal to 1.");
        }

        // Reject page sizes outside the allowed range.
        if (resolvedPageSize < 1 || resolvedPageSize > _paginationOptions.MaximumPageSize)
        {
            return BadRequest(
                $"PageSize must be between 1 and {_paginationOptions.MaximumPageSize}."
            );
        }
        //
        // Ask the Service Layer to retrieve the data.
        //
        // Notice:
        //
        // The controller does NOT know how to
        // query the database.
        //
        // It simply delegates that work to the service.
        //
        var requests = await _intakeService.GetRequestSummariesAsync(
            status,
            patient,
            sort,
            page,
            resolvedPageSize
        );

        //
        // Return HTTP 200 (OK)
        // along with the data.
        //
        // Equivalent to:
        //
        // Results.Ok(...)
        //
        return Ok(requests);
    }

    //
    // GET /requests/{id}
    //
    // Returns one intake request by ID.
    //
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        IntakeRequest? request = await _intakeService.FindRequestByIdAsync(id);

        return request is not null ? Ok(request) : NotFound();
    }

    //
    // POST /requests
    //
    // Creates a new intake request.
    //
    [HttpPost]
    public async Task<IActionResult> Create(CreateRequestDto dto)
    {
        IntakeRequest? request = await _intakeService.AddRequestAsync(dto.PatientId);

        if (request is null)
        {
            return BadRequest($"Patient with ID {dto.PatientId} does not exist.");
        }

        var response = new IntakeRequestResponseDto
        {
            Id = request.Id,
            PatientId = request.PatientId,
            ClinicId = request.ClinicId,
            Status = request.Status,
        };

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                // Use the version from the incoming POST request
                // when generating the new resource's URL.
                version = HttpContext.GetRequestedApiVersion()?.ToString(),

                // Supply the {id} required by GetById's route.
                id = request.Id,
            },
            response
        );
    }

    //
    // PUT /requests/{id}/status
    //
    // Updates the status of an existing intake request.
    //
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateRequestStatusDto dto)
    {
        Console.WriteLine($"Received request to update status of request {id} to {dto.Status}");
        bool updated = await _intakeService.UpdateStatusAsync(id, dto.Status);

        return updated ? NoContent() : NotFound();
    }

    //
    // DELETE /requests/{id}
    //
    // Deletes an intake request.
    //
    // Requires an authenticated user whose Role claim is "Admin".
    // An authenticated non-admin user receives 403 Forbidden.
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await _intakeService.DeleteRequestAsync(id);

        return deleted ? NoContent() : NotFound();
    }

    // to test exception handling middleware
    [HttpGet("error")]
    public IActionResult Error()
    {
        throw new Exception("This is a test exception.");
    }
}

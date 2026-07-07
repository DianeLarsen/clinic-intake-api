using ClinicIntakeApi.Dtos;
using ClinicIntakeApi.Models;
using ClinicIntakeApi.Services;
using Microsoft.AspNetCore.Mvc;

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
[Route("[controller]")]
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

    //
    // Constructor
    //
    // ASP.NET's Dependency Injection system automatically
    // creates an IntakeService and passes it here.
    //
    // We save it so every action in this controller can use it.
    //
    public RequestsController(IIntakeService intakeService)
    {
        _intakeService = intakeService;
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
        int pageSize = 10
    )
    {
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
            pageSize
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

        return Created($"/requests/{request.Id}", request);
    }

    //
    // PUT /requests/{id}/status
    //
    // Updates the status of an existing intake request.
    //
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateRequestStatusDto dto)
    {
        bool updated = await _intakeService.UpdateStatusAsync(id, dto.Status);

        return updated ? NoContent() : NotFound();
    }

    //
    // DELETE /requests/{id}
    //
    // Deletes an intake request.
    //
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await _intakeService.DeleteRequestAsync(id);

        return deleted ? NoContent() : NotFound();
    }
}

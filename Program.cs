using System.Text.Json.Serialization;
using ClinicIntakeApi.Data;
using ClinicIntakeApi.Dtos;
using ClinicIntakeApi.Filters;
using ClinicIntakeApi.Models;
using ClinicIntakeApi.Repositories;
using ClinicIntakeApi.Services;
using Microsoft.EntityFrameworkCore;

// Creates the application builder.
// This is where services and application configuration are registered.
var builder = WebApplication.CreateBuilder(args);

//
// Configure JSON serialization
// Converts enums like RequestStatus.Submitted into
// "Submitted" instead of 0.
//
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

//
// Register Swagger/OpenAPI services.
// These generate interactive API documentation.
//
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//
// Register Entity Framework Core.
//
// AddDbContext() creates one DbContext per HTTP request (Scoped).
// EF will use SQLite as the database.
//
builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite("Data Source=clinic-intake.db")
);

//
// Dependency Injection registrations.
//
// Whenever something asks for IIntakeRepository,
// create an EfIntakeRepository.
//
// Whenever something asks for IIntakeService,
// create an IntakeService.
//
builder.Services.AddScoped<IIntakeRepository, EfIntakeRepository>();
builder.Services.AddScoped<IIntakeService, IntakeService>();

// Build the application after all services have been registered.
var app = builder.Build();

//
// Only enable Swagger while developing.
//
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//
// Redirect HTTP requests to HTTPS.
//
app.UseHttpsRedirection();

//
// GET /requests
//
// Supports:
//
// Filtering
// Searching
// Sorting
// Pagination
//
// Returns a paged list of RequestSummaryDto.
//
app.MapGet(
    "/requests",
    async (
        IIntakeService intakeService,
        RequestStatus? status,
        string? patient,
        string? sort,
        int page = 1,
        int pageSize = 10
    ) =>
    {
        return Results.Ok(
            await intakeService.GetRequestSummariesAsync(status, patient, sort, page, pageSize)
        );
    }
);

//
// GET /requests/{id}
//
// Returns one request by ID.
//
app.MapGet(
    "/requests/{id}",
    async (int id, IIntakeService intakeService) =>
    {
        IntakeRequest? request = await intakeService.FindRequestByIdAsync(id);

        return request is not null ? Results.Ok(request) : Results.NotFound();
    }
);

//
// POST /requests
//
// Creates a new intake request.
//
// Validation happens BEFORE this endpoint executes
// using the ValidationFilter below.
//
app.MapPost(
        "/requests",
        async (CreateRequestDto dto, IIntakeService intakeService) =>
        {
            IntakeRequest request = await intakeService.AddRequestAsync(dto.PatientName);

            return Results.Created($"/requests/{request.Id}", request);
        }
    )
    // Runs the generic validation filter.
    // If validation fails, the endpoint never executes.
    .AddEndpointFilter<ValidationFilter<CreateRequestDto>>();

//
// PUT /requests/{id}/status
//
// Updates only the request status.
//
app.MapPut(
    "/requests/{id}/status",
    async (int id, UpdateRequestStatusDto dto, IIntakeService intakeService) =>
    {
        bool updated = await intakeService.UpdateStatusAsync(id, dto.Status);

        return updated ? Results.NoContent() : Results.NotFound();
    }
);

//
// DELETE /requests/{id}
//
// Deletes a request.
//
app.MapDelete(
    "/requests/{id}",
    async (int id, IIntakeService intakeService) =>
    {
        bool deleted = await intakeService.DeleteRequestAsync(id);

        return deleted ? Results.NoContent() : Results.NotFound();
    }
);

//
// Development Seed Data
//
// Creates sample data only if the database is empty.
//
// CreateScope() is required because DbContext is Scoped.
// Outside an HTTP request we must create our own scope.
//
using (var scope = app.Services.CreateScope())
{
    IIntakeService intakeService = scope.ServiceProvider.GetRequiredService<IIntakeService>();

    IEnumerable<IntakeRequest> existingRequests = await intakeService.GetAllRequestsAsync();

    // Only seed if this is a brand-new database.
    if (!existingRequests.Any())
    {
        string[] names =
        [
            "Alice",
            "Bob",
            "Charlie",
            "Diane",
            "Emma",
            "Frank",
            "Grace",
            "Henry",
            "Isabella",
            "Jack",
            "Karen",
            "Liam",
            "Mia",
            "Noah",
            "Olivia",
            "Patrick",
            "Quinn",
            "Ryan",
            "Sophia",
            "Tyler",
        ];

        // Create sample requests.
        for (int i = 0; i < names.Length; i++)
        {
            IntakeRequest request = await intakeService.AddRequestAsync(names[i]);

            // Give some requests different statuses
            // so filtering and searching have useful data.
            if (i % 3 == 0)
            {
                await intakeService.UpdateStatusAsync(request.Id, RequestStatus.InReview);
            }
            else if (i % 5 == 0)
            {
                await intakeService.UpdateStatusAsync(request.Id, RequestStatus.Completed);
            }
        }
    }
}

//
// Start listening for HTTP requests.
//
// Nothing happens until app.Run() is called.
//
app.Run();

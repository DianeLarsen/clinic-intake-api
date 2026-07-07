using System.Text.Json.Serialization;
using ClinicIntakeApi.Data;
using ClinicIntakeApi.Dtos;
// using ClinicIntakeApi.Filters;
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
builder.Services.AddControllers();

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
// app.MapGet(
//     "/requests",
//     async (
//         IIntakeService intakeService,
//         RequestStatus? status,
//         string? patient,
//         string? sort,
//         int page = 1,
//         int pageSize = 10
//     ) =>
//     {
//         return Results.Ok(
//             await intakeService.GetRequestSummariesAsync(status, patient, sort, page, pageSize)
//         );
//     }
// );

//
// GET /requests/{id}
//
// Returns one request by ID.
//
// app.MapGet(
//     "/requests/{id}",
//     async (int id, IIntakeService intakeService) =>
//     {
//         IntakeRequest? request = await intakeService.FindRequestByIdAsync(id);

//         return request is not null ? Results.Ok(request) : Results.NotFound();
//     }
// );

//
// POST /requests
//
// Creates a new intake request.
//
// Validation happens BEFORE this endpoint executes
// using the ValidationFilter below.
//
// app.MapPost(
//         "/requests",
//         async (CreateRequestDto dto, IIntakeService intakeService) =>
//         {
//             IntakeRequest request = await intakeService.AddRequestAsync(dto.PatientName, 1);

//             return Results.Created($"/requests/{request.Id}", request);
//         }
//     )
//     // Runs the generic validation filter.
//     // If validation fails, the endpoint never executes.
//     .AddEndpointFilter<ValidationFilter<CreateRequestDto>>();

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
// Development Seed / Backfill Data
//
// This block creates sample clinics, patients, and intake requests
// for local development.
//
// Clinics are created first because patients require a valid ClinicId.
// Patients are created next because intake requests require a valid PatientId.
//
using (var scope = app.Services.CreateScope())
{
    IIntakeService intakeService = scope.ServiceProvider.GetRequiredService<IIntakeService>();

    ClinicIntakeDbContext db = scope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

    IEnumerable<IntakeRequest> existingRequests = await intakeService.GetAllRequestsAsync();

    //
    // Ensure default clinics exist.
    //
    // Clinics are parent records.
    // A clinic can exist even if it has no patients or requests yet.
    //
    if (!db.Clinics.Any())
    {
        db.Clinics.AddRange(
            new Clinic { Name = "Monroe Clinic" },
            new Clinic { Name = "Everett Clinic" },
            new Clinic { Name = "Seattle Clinic" }
        );

        await db.SaveChangesAsync();
    }

    Clinic[] clinics = db.Clinics.ToArray();

    //
    // Sample patient identity data used for development.
    //
    var patientSeeds = new[]
    {
        new
        {
            FirstName = "Alice",
            LastName = "Johnson",
            DateOfBirth = new DateOnly(1984, 3, 12),
        },
        new
        {
            FirstName = "Bob",
            LastName = "Smith",
            DateOfBirth = new DateOnly(1979, 7, 22),
        },
        new
        {
            FirstName = "Charlie",
            LastName = "Brown",
            DateOfBirth = new DateOnly(1991, 11, 5),
        },
        new
        {
            FirstName = "Diane",
            LastName = "Larsen",
            DateOfBirth = new DateOnly(1978, 6, 14),
        },
        new
        {
            FirstName = "Emma",
            LastName = "Davis",
            DateOfBirth = new DateOnly(2002, 5, 9),
        },
        new
        {
            FirstName = "Frank",
            LastName = "Miller",
            DateOfBirth = new DateOnly(1968, 12, 1),
        },
    };

    //
    // Ensure default patients exist.
    //
    // Patients are child records of clinics.
    // Every patient must belong to a valid clinic.
    //
    if (!db.Patients.Any())
    {
        Patient[] patientsToCreate = patientSeeds
            .Select(
                (patient, index) =>
                    new Patient
                    {
                        FirstName = patient.FirstName,
                        LastName = patient.LastName,
                        DateOfBirth = patient.DateOfBirth,

                        // Rotate patients across available clinics.
                        ClinicId = clinics[index % clinics.Length].Id,
                    }
            )
            .ToArray();

        db.Patients.AddRange(patientsToCreate);

        await db.SaveChangesAsync();
    }

    //
    // Load patients after ensuring they exist.
    //
    // These are used to create sample intake requests.
    //
    Patient[] patients = db.Patients.ToArray();

    //
    // If there are no requests at all, create sample intake requests.
    //
    // This runs only for a brand-new database.
    //
    if (!existingRequests.Any())
    {
        for (int i = 0; i < patients.Length; i++)
        {
            Patient patient = patients[i];

            IntakeRequest? request = await intakeService.AddRequestAsync(patient.Id);

            if (request is null)
            {
                continue;
            }

            //
            // Give some requests different statuses
            // so filtering, sorting, and searching have useful sample data.
            //
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

app.MapControllers();

//
// Start listening for HTTP requests.
//
// Nothing happens until app.Run() is called.
//
app.Run();

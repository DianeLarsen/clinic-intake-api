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
// This block does two jobs:
//
// 1. If the database is brand new, it creates sample clinics,
//    patients, and intake requests.
//
// 2. If the database already exists but has missing relationship data,
//    it repairs/backfills that data.
//
// This is closer to a real migration mindset than deleting the database
// every time the model changes. Tiny mercy from the database goblin.
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
    // Sample patient names used for development data.
    //
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

    //
    // Ensure default patients exist.
    //
    // Patients are child records of clinics.
    // Every patient must belong to a valid clinic.
    //
    if (!db.Patients.Any())
    {
        Patient[] patientsToCreate = names
            .Select(
                (name, index) =>
                    new Patient { FullName = name, ClinicId = clinics[index % clinics.Length].Id }
            )
            .ToArray();

        db.Patients.AddRange(patientsToCreate);

        await db.SaveChangesAsync();
    }

    // Load patients from the database after ensuring they exist.
    // This array is used for both backfill logic and sample request creation.
    Patient[] patients = db.Patients.ToArray();

    // //
    // // Backfill existing requests that do not have a PatientId.
    // //
    // // This handles older rows created before the Patient relationship existed.
    // // Instead of deleting the database, we assign each old request to an
    // // existing patient and keep ClinicId consistent with that patient's clinic.
    // //
    // int backfillIndex = 0;

    // foreach (IntakeRequest request in existingRequests)
    // {
    //     if (request.PatientId is null)
    //     {
    //         Patient patient = patients[backfillIndex % patients.Length];

    //         request.PatientId = patient.Id;
    //         request.ClinicId = patient.ClinicId;

    //         backfillIndex++;
    //     }
    // }

    await db.SaveChangesAsync();

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

            IntakeRequest request = await intakeService.AddRequestAsync(
                patient.FullName,
                patient.ClinicId,
                patient.Id
            );

            // Connect the intake request to the patient.
            request.PatientId = patient.Id;

            await db.SaveChangesAsync();

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

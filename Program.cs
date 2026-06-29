using System.Text.Json.Serialization;
using ClinicIntakeApi.Data;
using ClinicIntakeApi.Dtos;
using ClinicIntakeApi.Models;
using ClinicIntakeApi.Repositories;
using ClinicIntakeApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ClinicIntakeDbContext>(options =>
    options.UseSqlite("Data Source=clinic-intake.db")
);

builder.Services.AddScoped<IIntakeRepository, EfIntakeRepository>();

builder.Services.AddScoped<IIntakeService, IntakeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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

app.MapGet(
    "/requests/{id}",
    async (int id, IIntakeService intakeService) =>
    {
        IntakeRequest? request = await intakeService.FindRequestByIdAsync(id);

        return request is not null ? Results.Ok(request) : Results.NotFound();
    }
);

app.MapPost(
    "/requests",
    async (CreateRequestDto dto, IIntakeService intakeService) =>
    {
        if (string.IsNullOrWhiteSpace(dto.PatientName))
        {
            return Results.BadRequest("Patient name is required.");
        }
        IntakeRequest request = await intakeService.AddRequestAsync(dto.PatientName);

        return Results.Created($"/requests/{request.Id}", request);
    }
);

app.MapPut(
    "/requests/{id}/status",
    async (int id, UpdateRequestStatusDto dto, IIntakeService intakeService) =>
    {
        bool updated = await intakeService.UpdateStatusAsync(id, dto.Status);

        return updated ? Results.NoContent() : Results.NotFound();
    }
);

app.MapDelete(
    "/requests/{id}",
    async (int id, IIntakeService intakeService) =>
    {
        bool deleted = await intakeService.DeleteRequestAsync(id);

        return deleted ? Results.NoContent() : Results.NotFound();
    }
);

using (var scope = app.Services.CreateScope())
{
    IIntakeService intakeService = scope.ServiceProvider.GetRequiredService<IIntakeService>();
    IEnumerable<IntakeRequest> existingRequests = await intakeService.GetAllRequestsAsync();

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

        for (int i = 0; i < names.Length; i++)
        {
            IntakeRequest request = await intakeService.AddRequestAsync(names[i]);

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

app.Run();

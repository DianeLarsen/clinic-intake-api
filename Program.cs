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
    (
        IIntakeService intakeService,
        RequestStatus? status,
        string? patient,
        string? sort,
        int page = 1,
        int pageSize = 10
    ) =>
    {
        return intakeService.GetRequests(status, patient, sort, page, pageSize);
    }
);

app.MapGet(
    "/requests/{id}",
    (int id, IIntakeService intakeService) =>
    {
        IntakeRequest? request = intakeService.FindRequestById(id);

        return request is not null ? Results.Ok(request) : Results.NotFound();
    }
);

app.MapPost(
    "/requests",
    (CreateRequestDto dto, IIntakeService intakeService) =>
    {
        if (string.IsNullOrWhiteSpace(dto.PatientName))
        {
            return Results.BadRequest("Patient name is required.");
        }
        IntakeRequest request = intakeService.AddRequest(dto.PatientName);

        return Results.Created($"/requests/{request.Id}", request);
    }
);

app.MapPut(
    "/requests/{id}/status",
    (int id, UpdateRequestStatusDto dto, IIntakeService intakeService) =>
    {
        bool updated = intakeService.UpdateStatus(id, dto.Status);

        return updated ? Results.NoContent() : Results.NotFound();
    }
);

app.MapDelete(
    "/requests/{id}",
    (int id, IIntakeService intakeService) =>
    {
        bool deleted = intakeService.DeleteRequest(id);

        return deleted ? Results.NoContent() : Results.NotFound();
    }
);

using (var scope = app.Services.CreateScope())
{
    IIntakeService intakeService = scope.ServiceProvider.GetRequiredService<IIntakeService>();

    if (!intakeService.GetAllRequests().Any())
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
            IntakeRequest request = intakeService.AddRequest(names[i]);

            if (i % 3 == 0)
            {
                intakeService.UpdateStatus(request.Id, RequestStatus.InReview);
            }
            else if (i % 5 == 0)
            {
                intakeService.UpdateStatus(request.Id, RequestStatus.Completed);
            }
        }
    }
}

app.Run();
